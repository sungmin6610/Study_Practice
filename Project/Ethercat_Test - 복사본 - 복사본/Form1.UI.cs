using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;
using EtherCAT_Test.Common;
using EtherCAT_Test.Logging;

namespace Ethercat_Test
{
    // ─────────────────────────────────────────────────────────────
    //  HMI 전용 partial (Form1.cs 는 손대지 않음).
    //  같은 Form1 클래스이므로 private 매니저 필드(io, motion, robot,
    //  chamber, waferManager, alarm, equipment, autoSequence)에 접근 가능.
    //  갱신은 별도 uiTimer 로 수행 → timer1_Tick / autoSequence.Run() 구조 불변.
    // ─────────────────────────────────────────────────────────────
    public partial class Form1
    {
        private Label[] _slotA;
        private Label[] _slotB;
        private bool _blink;

        // 챔버 공정 진행률 계산용 (A/B/C = 0/1/2): Busy 시작 시각 보관
        private readonly DateTime[] _procStart = new DateTime[3];
        private readonly bool[] _procRunning = new bool[3];

        private static readonly Color SlotOn = Color.RoyalBlue;
        private static readonly Color SlotOff = Color.Gainsboro;

        private void Form1_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;

            _slotA = new[] { slotA1, slotA2, slotA3, slotA4, slotA5 };
            _slotB = new[] { slotB1, slotB2, slotB3, slotB4, slotB5 };

            PopulateParameterTab();
            InitRecipeTab();
            InitLoggingHooks();   // Phase 2: 로깅 훅 배선
            SetupAlarmList();     // Phase 2: 알람 이력 ListView(시각/스텝/메시지)
            InitE10Tab();         // Phase 3: E10 / Lot 탭
            InitLogTab();         // 수정 3: 실시간 Log 탭
            InitAlarmBanner();    // 수정 1: 알람 배너
            InitAuthUI();         // 로그인/권한: 헤더 사용자 + 로그아웃
            InitAdminTab();       // 관리자 탭
            ApplyModernTheme();   // 세련된 테마 (마지막에 일괄 적용)
            UpdateControlEnable(); // 상태+권한 기반 버튼 상태 적용
            UpdateMimicCache();    // 미믹 초기 표시용 캐시
        }

        // 알람 탭 ListView 컬럼 구성 + 오늘자 CSV 로드분 초기 표시
        private void SetupAlarmList()
        {
            lvAlarm.Columns.Clear();
            lvAlarm.Columns.Add("시각", 150);
            lvAlarm.Columns.Add("스텝", 240);
            lvAlarm.Columns.Add("메시지", 1150);
            SyncAlarmList();
        }

        // LogManager.AlarmHistory 와 ListView 동기화 (최신이 위로). 건수 변화 시에만 재구성.
        private void SyncAlarmList()
        {
            var h = LogManager.Instance.AlarmHistory;
            if (lvAlarm.Items.Count == h.Count) return;
            lvAlarm.BeginUpdate();
            lvAlarm.Items.Clear();
            for (int i = h.Count - 1; i >= 0; i--)
            {
                var a = h[i];
                var it = new ListViewItem(a.Time.ToString("HH:mm:ss"));
                it.SubItems.Add(a.Step ?? "-");
                it.SubItems.Add(a.Message ?? "");
                it.ForeColor = Color.Firebrick;
                lvAlarm.Items.Add(it);
            }
            lvAlarm.EndUpdate();
        }

        // Parameter 탭: Position.cs 티칭 좌표 읽기전용 표시
        private void PopulateParameterTab()
        {
            var sb = new StringBuilder();
            sb.AppendLine("■ 좌/우(LR) 축 티칭 위치");
            sb.AppendLine($"    FOUP A          = {Position.FOUPA_X,12:N0}");
            sb.AppendLine($"    FOUP B          = {Position.FOUPB_X,12:N0}");
            sb.AppendLine($"    CHAMBER A       = {Position.CHAMBER_A_X,12:N0}");
            sb.AppendLine($"    CHAMBER B       = {Position.CHAMBER_B_X,12:N0}");
            sb.AppendLine($"    CHAMBER C       = {Position.CHAMBER_C_X,12:N0}");
            sb.AppendLine();
            sb.AppendLine("■ 상/하(UD) 축 - 챔버 공통");
            sb.AppendLine($"    상승(이동)      = {Position.CHAMBER_UP,12:N0}");
            sb.AppendLine($"    안착            = {Position.CHAMBER_DOWN,12:N0}");
            sb.AppendLine();
            sb.AppendLine("■ 상/하(UD) 축 - FOUP 슬롯 (상승 / 안착)");
            for (int s = 1; s <= 5; s++)
                sb.AppendLine($"    Slot {s}  :  {Position.FOUPA_UP[s],12:N0}  /  {Position.FOUPA_DOWN[s],12:N0}");
            sb.AppendLine();
            sb.AppendLine("※ 서보 가감속/속도 파라미터는 Manual 탭에서 설정/적용합니다.");

            lblParamText.Text = sb.ToString();
        }

        private void uiTimer_Tick(object sender, EventArgs e)
        {
            _blink = !_blink;
            lblClock.Text = DateTime.Now.ToString("yyyy-MM-dd  HH:mm:ss");

            if (_slotA == null) return;

            try { UpdateHmi(); }
            catch { /* 연결 전/일시적 읽기 오류는 무시 */ }

            mimicPanel.Invalidate();
        }

        private void UpdateHmi()
        {
            // ── 로드포트 슬롯 인디케이터 ──
            UpdateSlotIndicators();

            lblLoadAStatus.Text = autoSequence.AutoRun ? "RUN" : equipment.CurrentState.ToString();
            lblLoadBStatus.Text = equipment.CurrentState == EquipmentState.Complete ? "DONE" : "IDLE";

            // ── 타워 램프 인디케이터 (출력 로직과 동일 규칙) ──
            bool red = equipment.IsAlarm || equipment.IsEmergency;
            bool green = autoSequence.AutoRun && !red;
            bool yellow = !red && !green;
            towerRed.BackColor = red ? (_blink ? Color.Red : Color.DarkRed) : Color.FromArgb(70, 20, 20);
            towerYellow.BackColor = yellow ? Color.Gold : Color.FromArgb(70, 65, 20);
            towerGreen.BackColor = green ? Color.LimeGreen : Color.FromArgb(20, 70, 30);

            // ── 챔버 패널 인디케이터 ──
            UpdateChamber(0, chamber.ChamberA, pbChamA, lblWaferA, lblLampA);
            UpdateChamber(1, chamber.ChamberB, pbChamB, lblWaferB, lblLampB);
            UpdateChamber(2, chamber.ChamberC, pbChamC, lblWaferC, lblLampC);

            // ── 알람 이력: LogManager.AlarmHistory 기준으로 동기화 ──
            SyncAlarmList();

            // ── Phase 3: E10 상태 집계 / Lot 정보 갱신 ──
            UpdateE10AndLot();
        }

        // 로드포트 슬롯 인디케이터 갱신 (WaferManager 기준). timer1_Tick / uiTimer 공용.
        private void UpdateSlotIndicators()
        {
            if (_slotA == null || _slotB == null) return;
            for (int i = 1; i <= 5; i++)
            {
                var wafer = waferManager.GetWafer(i);
                _slotA[i - 1].BackColor = (wafer != null && wafer.Location == WaferLocation.FOUPA) ? SlotOn : SlotOff;
                _slotB[i - 1].BackColor = (wafer != null && wafer.Location == WaferLocation.FOUPB) ? SlotOn : SlotOff;
            }
        }

        private void UpdateChamber(int idx, EtherCAT_Test.Chamber.Chamber cham,
                                   ProgressBar pb, Label wafer, Label lamp)
        {
            // 공정 진행 ProgressBar = (경과시간 / 현재 레시피 공정시간) %
            if (pb.Style != ProgressBarStyle.Blocks) pb.Style = ProgressBarStyle.Blocks;

            if (cham.Busy)
            {
                if (!_procRunning[idx]) { _procRunning[idx] = true; _procStart[idx] = DateTime.Now; }
                int total = cham.ProcessTimeMs;               // 현재 레시피의 공정시간
                double elapsed = (DateTime.Now - _procStart[idx]).TotalMilliseconds;
                int pct = total > 0 ? (int)(elapsed / total * 100.0) : 0;
                if (pct < 0) pct = 0; if (pct > 100) pct = 100;
                pb.Value = pct;
            }
            else
            {
                _procRunning[idx] = false;
                pb.Value = 0;
            }

            wafer.Text = cham.HasWafer ? "WAFER" : "-";
            wafer.ForeColor = cham.HasWafer ? Color.RoyalBlue : Color.Gray;

            lamp.Text = cham.Busy ? "PROCESS" : "IDLE";
            lamp.ForeColor = cham.Busy ? Color.OrangeRed : Color.Gray;
        }

        // ── 미믹 다이어그램(2축 + 도어/웨이퍼)은 Form1.Mimic.cs 로 이동 (수정 3) ──
    }
}
