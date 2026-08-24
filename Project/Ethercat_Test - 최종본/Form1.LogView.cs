using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using EtherCAT_Test.Logging;

namespace Ethercat_Test
{
    // ─────────────────────────────────────────────────────────────
    //  실시간 Log 뷰 탭 (코드 생성 → Form1.Designer.cs 미변경).
    //  LogManager.OnLog 구독, 새 로그를 맨 위에 삽입, 레벨별 색상,
    //  최대 500행 유지(UI만), 시작 시 오늘자 event.csv 마지막 100행 표시.
    // ─────────────────────────────────────────────────────────────
    public partial class Form1
    {
        private ListView lvLog;
        private CheckBox chkAutoScroll;
        private const int LogMaxRows = 500;

        private void InitLogTab()
        {
            var tab = new TabPage("Log") { BackColor = Color.White };

            // 상단 툴바
            var bar = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = Color.WhiteSmoke };
            var btnClear = new Button { Text = "지우기", Location = new Point(10, 8), Size = new Size(90, 28) };
            var btnOpen = new Button { Text = "로그 폴더 열기", Location = new Point(108, 8), Size = new Size(130, 28) };
            chkAutoScroll = new CheckBox { Text = "자동 스크롤", Location = new Point(256, 12), AutoSize = true, Checked = true };
            btnClear.Click += (s, e) => { lvLog.Items.Clear(); };
            btnOpen.Click += (s, e) =>
            {
                try { Process.Start(LogManager.Instance.LogDir); } catch { }
            };
            bar.Controls.Add(btnClear);
            bar.Controls.Add(btnOpen);
            bar.Controls.Add(chkAutoScroll);

            // 로그 리스트
            lvLog = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                Font = new Font("Consolas", 10F)
            };
            lvLog.Columns.Add("시각", 150);
            lvLog.Columns.Add("레벨", 70);
            lvLog.Columns.Add("스텝", 220);
            lvLog.Columns.Add("메시지", 1080);

            // Fill 컨트롤을 먼저 추가(도킹 우선순위) → 이후 Top 바
            tab.Controls.Add(lvLog);
            tab.Controls.Add(bar);
            tabMain.TabPages.Add(tab);

            // 오늘자 event.csv 마지막 100행 초기 표시 (오래된→최신 순으로 받아 0에 삽입 → 최신이 위)
            foreach (var row in LogManager.Instance.ReadRecentEvents(100))
                InsertLogRow(row[0], row[1], row[2], row[3]);

            // 실시간 구독
            LogManager.Instance.OnLog += OnLogReceived;
            this.FormClosing += (s, e) => { LogManager.Instance.OnLog -= OnLogReceived; };
        }

        // LogManager.OnLog 핸들러 — 어느 스레드에서 오든 UI 스레드로 마샬링
        private void OnLogReceived(string level, string step, string message)
        {
            if (lvLog == null || lvLog.IsDisposed) return;
            if (lvLog.InvokeRequired)
            {
                try { lvLog.BeginInvoke((Action)(() => AddLiveLog(level, step, message))); } catch { }
                return;
            }
            AddLiveLog(level, step, message);
        }

        private void AddLiveLog(string level, string step, string message)
        {
            InsertLogRow(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), level, step, message);
        }

        // 한 행을 맨 위(index 0)에 삽입 + 색상 + 500행 캡 + 자동스크롤
        private void InsertLogRow(string time, string level, string step, string message)
        {
            if (lvLog == null || lvLog.IsDisposed) return;

            // 시각은 시:분:초.밀리만 간결히 표시
            string t = time;
            int sp = time.IndexOf(' ');
            if (sp >= 0 && sp + 1 < time.Length) t = time.Substring(sp + 1);

            var it = new ListViewItem(t);
            it.SubItems.Add(level ?? "");
            it.SubItems.Add(step ?? "");
            it.SubItems.Add(message ?? "");

            switch (level)
            {
                case "ALARM": it.ForeColor = Color.Red; break;
                case "STATE": it.ForeColor = Color.Blue; break;
                default: it.ForeColor = Color.Black; break;
            }

            lvLog.Items.Insert(0, it);

            while (lvLog.Items.Count > LogMaxRows)
                lvLog.Items.RemoveAt(lvLog.Items.Count - 1);

            if (chkAutoScroll != null && chkAutoScroll.Checked && lvLog.Items.Count > 0)
                lvLog.Items[0].EnsureVisible();
        }
    }
}
