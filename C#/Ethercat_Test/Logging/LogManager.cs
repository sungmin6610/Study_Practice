using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EtherCAT_Test.Common;

namespace EtherCAT_Test.Logging
{
    // ─────────────────────────────────────────────────────────────
    //  [이 파일이 하는 일]
    //  프로그램에서 일어나는 일(상태변화·알람·웨이퍼 공정 등)을 CSV 파일로 기록한다.
    //  CSV = 콤마(,)로 칸을 나눈 표 형식 텍스트. 엑셀로 바로 열 수 있다.
    //  Logs 폴더에 날짜별로 파일이 쌓인다. (예: 2026-07-07_event.csv)
    //  LogManager 도 싱글턴(Instance 하나)이며, 파일 쓰기 실패해도 프로그램이 멈추지
    //  않도록 전부 try-catch 로 감싼다(장비 제어가 로그보다 우선이므로).
    // ─────────────────────────────────────────────────────────────

    // 알람 한 건의 기록(발생 시각/발생 스텝/메시지). 알람 이력 목록에 쌓인다.
    public class AlarmEntry
    {
        public DateTime Time;
        public string Step;
        public string Message;
    }

    /// <summary>
    /// CSV 이벤트/웨이퍼 로깅.
    /// 접근 방식: static 싱글턴(LogManager.Instance) 유지.
    ///  - 이유: 기존 매니저(AlarmManager / EquipmentStateManager / AutoSequence)의 로깅 훅이
    ///    이미 LogManager.Instance 로 배선되어 있어, 생성자 주입으로 바꾸면 다수 클래스의
    ///    생성자 시그니처(및 AutoSequence)를 손대야 하므로 가장 덜 침습적인 static 접근을 유지함.
    ///  - "Form1 소유 단일 인스턴스" 요구도 단일 인스턴스라는 점에서 동일하게 충족.
    ///
    /// 스레드 안전(lock), Logs 폴더 자동 생성, append 모드, 헤더 자동 작성,
    /// CSV 이스케이프, 쓰기 실패는 내부 try-catch 로 흡수(시퀀스 보호).
    /// 경로는 AppDomain.CurrentDomain.BaseDirectory/Logs 기준.
    /// </summary>
    public class LogManager
    {
        private static readonly LogManager _instance = new LogManager();
        public static LogManager Instance => _instance;

        private readonly string _dir;
        private readonly object _lock = new object();
        private readonly Dictionary<string, DateTime> _waferIn = new Dictionary<string, DateTime>();

        /// <summary>로그 기록 시 발생 (level, step, message). Log 창이 구독.</summary>
        public event Action<string, string, string> OnLog;

        /// <summary>알람 로그 시점의 시퀀스 스텝 provider (Form1 에서 주입).</summary>
        public Func<string> StepProvider;

        /// <summary>현재 Lot ID (외부에서 설정, 미설정 시 "-").</summary>
        public string CurrentLotId = "-";

        /// <summary>알람 이력(오늘자 CSV 로드분 + 런타임). 최신은 마지막.</summary>
        public List<AlarmEntry> AlarmHistory { get; } = new List<AlarmEntry>();

        /// <summary>Logs 폴더 절대경로 (탐색기 열기용).</summary>
        public string LogDir => _dir;

        private const string EventHeader = "Time,Level,Step,Message";
        private const string WaferHeader = "Time,Slot,Chamber,Process,StartTime,EndTime,ElapsedSec";

        private LogManager()
        {
            _dir = JsonUtil.Combine("Logs");
            try { if (!Directory.Exists(_dir)) Directory.CreateDirectory(_dir); } catch { }
            LoadTodayAlarms();
        }

        private string EventPath => Path.Combine(_dir, DateTime.Now.ToString("yyyy-MM-dd") + "_event.csv");
        private string WaferPath => Path.Combine(_dir, DateTime.Now.ToString("yyyy-MM-dd") + "_wafer.csv");

        // ══════════════ 신규 스펙 공개 API ══════════════

        public void Info(string message) => Write("INFO", "", message);

        public void State(string from, string to) => Write("STATE", "", $"{from} -> {to}");

        public void Alarm(string step, string message)
        {
            lock (_lock)
                AlarmHistory.Add(new AlarmEntry { Time = DateTime.Now, Step = step, Message = message });
            Write("ALARM", step, message);
        }

        /// <summary>웨이퍼 1스텝 공정 기록 → wafer.csv.</summary>
        public void WaferProcess(int slot, string chamber, string process, DateTime start, DateTime end)
        {
            double sec = Math.Round((end - start).TotalSeconds, 1);
            AppendCsv(WaferPath, WaferHeader,
                $"{Esc(Ts(DateTime.Now))},{slot},{Esc(chamber)},{Esc(process)}," +
                $"{Esc(Ts(start))},{Esc(Ts(end))},{sec}");
        }

        // ══════════════ 호환 유지용(기존 훅에서 사용) ══════════════

        public void LogEvent(string level, string message) => Write(level, "", message);

        public void LogState(string prev, string next) => State(prev, next);

        public void LogAlarm(string message)
        {
            string step = "-";
            try { step = StepProvider?.Invoke() ?? "-"; } catch { }
            Alarm(step, message);
        }

        /// <summary>공정 시작 시각을 내부 필드(_waferIn)에 보관.</summary>
        public void WaferProcessStart(int slot, string chamber, string process)
        {
            lock (_lock) _waferIn[slot + "|" + chamber] = DateTime.Now;
            Info($"공정 시작 - Slot {slot} / {chamber} ({process})");
        }

        /// <summary>보관해둔 시작 시각으로 WaferProcess 기록.</summary>
        public void WaferProcessEnd(int slot, string chamber, string process)
        {
            var now = DateTime.Now;
            DateTime start;
            lock (_lock) { if (!_waferIn.TryGetValue(slot + "|" + chamber, out start)) start = now; }
            WaferProcess(slot, chamber, process, start, now);
            Info($"공정 종료 - Slot {slot} / {chamber} ({process}) {Math.Round((now - start).TotalSeconds, 1)}s");
        }

        public void WaferUnloaded(int slot)
        {
            Info($"Slot {slot} FOUP B 안착 완료");
            EventReport(2001, "WaferDone", $"slot={slot}");
        }

        public void EventReport(int eventId, string name, string data)
            => Info($"EVENT CEID={eventId} {name}" + (string.IsNullOrEmpty(data) ? "" : " | " + data));

        // ══════════════ 초기 로드 / 조회 ══════════════

        /// <summary>오늘자 event.csv 마지막 maxRows 행을 [Time,Level,Step,Message] 로 반환(오래된→최신).</summary>
        public List<string[]> ReadRecentEvents(int maxRows)
        {
            var result = new List<string[]>();
            try
            {
                var path = EventPath;
                if (!File.Exists(path)) return result;
                string[] lines;
                lock (_lock) lines = File.ReadAllLines(path, Encoding.UTF8);
                int start = Math.Max(1, lines.Length - maxRows);   // 0행=헤더 skip
                for (int i = start; i < lines.Length; i++)
                {
                    var c = SplitCsv(lines[i]);
                    if (c.Count < 4) continue;
                    result.Add(new[] { c[0], c[1], c[2], c[3] });
                }
            }
            catch { }
            return result;
        }

        // ══════════════ 내부 구현 ══════════════

        private void Write(string level, string step, string message)
        {
            AppendCsv(EventPath, EventHeader,
                $"{Esc(Ts(DateTime.Now))},{Esc(level)},{Esc(step)},{Esc(message)}");

            var h = OnLog;   // 이벤트는 lock 밖에서 발생 (핸들러가 UI 마샬링)
            if (h != null)
            {
                try { h(level, step, message); } catch { }
            }
        }

        private void AppendCsv(string path, string header, string line)
        {
            try
            {
                lock (_lock)
                {
                    bool exists = File.Exists(path);
                    using (var sw = new StreamWriter(path, true, Encoding.UTF8))
                    {
                        if (!exists) sw.WriteLine(header);
                        sw.WriteLine(line);
                    }
                }
            }
            catch { /* 로깅 실패는 무시 (시퀀스 우선) */ }
        }

        private static string Ts(DateTime t) => t.ToString("yyyy-MM-dd HH:mm:ss.fff");

        // CSV 안전 처리(escape): 글자 안에 콤마(,)나 큰따옴표(")나 줄바꿈이 들어 있으면
        // 그 칸을 통째로 "..." 로 감싸고, 안의 " 는 "" 로 두 번 써서 표가 깨지지 않게 한다.
        // (CSV 규칙: 콤마는 칸 구분자이므로, 데이터 속 콤마와 헷갈리지 않게 보호하는 것)
        private static string Esc(string s)
        {
            if (s == null) return "";
            if (s.IndexOf(',') >= 0 || s.IndexOf('"') >= 0 || s.IndexOf('\n') >= 0 || s.IndexOf('\r') >= 0)
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }

        private void LoadTodayAlarms()
        {
            try
            {
                var path = EventPath;
                if (!File.Exists(path)) return;
                foreach (var raw in File.ReadAllLines(path, Encoding.UTF8))
                {
                    var c = SplitCsv(raw);
                    if (c.Count < 4 || c[1] != "ALARM") continue;   // Time,Level,Step,Message
                    DateTime t;
                    DateTime.TryParse(c[0], out t);
                    AlarmHistory.Add(new AlarmEntry { Time = t, Step = c[2], Message = c[3] });
                }
            }
            catch { }
        }

        private static List<string> SplitCsv(string line)
        {
            var res = new List<string>();
            if (line == null) return res;
            var sb = new StringBuilder();
            bool q = false;
            for (int i = 0; i < line.Length; i++)
            {
                char ch = line[i];
                if (q)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                        else q = false;
                    }
                    else sb.Append(ch);
                }
                else
                {
                    if (ch == '"') q = true;
                    else if (ch == ',') { res.Add(sb.ToString()); sb.Clear(); }
                    else sb.Append(ch);
                }
            }
            res.Add(sb.ToString());
            return res;
        }
    }
}
