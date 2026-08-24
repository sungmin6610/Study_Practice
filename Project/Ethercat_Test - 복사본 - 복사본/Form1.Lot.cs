using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EtherCAT_Test.Equipment;
using EtherCAT_Test.Logging;
using EtherCAT_Test.Process;

namespace Ethercat_Test
{
    // ─────────────────────────────────────────────────────────────
    //  Phase 3: Lot 라이프사이클 + SEMI E10 표시 (partial class Form1).
    // ─────────────────────────────────────────────────────────────
    public partial class Form1
    {
        private Lot currentLot;
        private Label lblE10;
        private Label lblLotInfo;

        // ── Lot 라이프사이클 (Form1.Log.cs 의 HandleAuto* 에서 호출) ──
        private void StartLot()
        {
            currentLot = new Lot(5);
            LogManager.Instance.CurrentLotId = currentLot.LotId;
            LogManager.Instance.EventReport(1001, "LotStart", currentLot.LotId);
        }

        private void EndLot(bool aborted)
        {
            if (currentLot == null) return;
            if (aborted) currentLot.Abort(); else currentLot.Complete();
            LogManager.Instance.EventReport(1002, "LotEnd", currentLot.LotId + " " + currentLot.State);
        }

        // ── E10 / Lot 탭 (코드 생성) ──
        private void InitE10Tab()
        {
            var tab = new TabPage("E10 / Lot") { BackColor = Color.White };

            var gbE10 = new GroupBox
            {
                Text = "SEMI E10 상태 집계",
                Location = new Point(20, 20),
                Size = new Size(560, 340),
                Font = new Font("굴림", 10F, FontStyle.Bold)
            };
            lblE10 = new Label
            {
                Location = new Point(20, 30),
                Size = new Size(520, 290),
                Font = new Font("Consolas", 12F),
                Text = "-"
            };
            gbE10.Controls.Add(lblE10);

            var gbLot = new GroupBox
            {
                Text = "현재 Lot",
                Location = new Point(600, 20),
                Size = new Size(480, 340),
                Font = new Font("굴림", 10F, FontStyle.Bold)
            };
            lblLotInfo = new Label
            {
                Location = new Point(20, 30),
                Size = new Size(440, 290),
                Font = new Font("Consolas", 12F),
                Text = "-"
            };
            gbLot.Controls.Add(lblLotInfo);

            tab.Controls.Add(gbE10);
            tab.Controls.Add(gbLot);
            tabMain.TabPages.Add(tab);
        }

        // ── 매 틱 갱신 (uiTimer 에서 호출) ──
        private void UpdateE10AndLot()
        {
            var snap = E10StateTracker.Instance.Snapshot();
            var sb = new StringBuilder();
            sb.AppendLine("[ E10 상태별 누적시간 ]");
            sb.AppendLine($"  Productive       : {snap[E10State.Productive],8:F1} s");
            sb.AppendLine($"  Standby          : {snap[E10State.Standby],8:F1} s");
            sb.AppendLine($"  Engineering      : {snap[E10State.Engineering],8:F1} s");
            sb.AppendLine($"  Scheduled Down   : {snap[E10State.ScheduledDown],8:F1} s");
            sb.AppendLine($"  Unscheduled Down : {snap[E10State.UnscheduledDown],8:F1} s");
            sb.AppendLine($"  Non-Scheduled    : {snap[E10State.NonScheduled],8:F1} s");
            sb.AppendLine();
            sb.AppendLine($"  가동률 Availability = {E10StateTracker.Instance.AvailabilityPercent():F1} %");
            if (lblE10 != null) lblE10.Text = sb.ToString();

            var lb = new StringBuilder();
            if (currentLot == null)
            {
                lb.AppendLine("진행 중인 Lot 없음");
            }
            else
            {
                lb.AppendLine($"Lot ID : {currentLot.LotId}");
                lb.AppendLine($"상태   : {currentLot.State}");
                lb.AppendLine($"시작   : {currentLot.StartTime:HH:mm:ss}");
                lb.AppendLine($"종료   : {(currentLot.EndTime.HasValue ? currentLot.EndTime.Value.ToString("HH:mm:ss") : "-")}");
                lb.AppendLine($"웨이퍼 : {currentLot.WaferCount} 매");
            }
            if (lblLotInfo != null) lblLotInfo.Text = lb.ToString();

            // Load Port A 패널 Status 를 Lot 정보와 연동
            if (currentLot != null)
                lblLoadAStatus.Text = $"{currentLot.State}  (start {currentLot.StartTime:HH:mm:ss})";
        }
    }
}
