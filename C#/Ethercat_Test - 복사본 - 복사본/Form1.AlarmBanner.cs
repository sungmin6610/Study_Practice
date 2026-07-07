using System;
using System.Drawing;
using System.Windows.Forms;
using EtherCAT_Test.Logging;

namespace Ethercat_Test
{
    // ─────────────────────────────────────────────────────────────
    //  수정 1: 알람 배너 + 상태 점멸 (partial class Form1).
    //  헤더 바로 아래 전폭 배너. 평시 숨김, 알람 시 표시+점멸.
    //  모든 갱신은 timer1_Tick 에서 호출(별도 스레드 없음).
    // ─────────────────────────────────────────────────────────────
    public partial class Form1
    {
        private Panel pnlAlarmBanner;
        private Label lblBannerIcon;
        private Label lblBannerMsg;
        private Button btnBannerReset;
        private bool _bannerVisible;

        // 점멸(5틱=500ms 토글). timer1_Tick 에서 갱신.
        private bool blinkFlag;
        private int blinkCounter;

        private static readonly Color AlarmRedDark = Color.FromArgb(0xC0, 0x39, 0x2B);
        private static readonly Color AlarmRedLite = Color.FromArgb(0xE7, 0x4C, 0x3C);

        private void InitAlarmBanner()
        {
            pnlAlarmBanner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                Visible = false,
                BackColor = AlarmRedDark
            };

            lblBannerMsg = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 12F, FontStyle.Bold),
                Padding = new Padding(10, 0, 0, 0),
                Text = ""
            };
            btnBannerReset = new Button
            {
                Dock = DockStyle.Right,
                Width = 170,
                Text = "Alarm Reset",
                ForeColor = AlarmRedDark,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("맑은 고딕", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            btnBannerReset.FlatAppearance.BorderSize = 0;
            btnBannerReset.Click += (s, e) => autoSequence.Reset();  // 기존 알람리셋 로직 재사용
            lblBannerIcon = new Label
            {
                Dock = DockStyle.Left,
                Width = 56,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 20F, FontStyle.Bold),
                Text = "⚠"
            };

            // Fill 먼저 추가(도킹 우선순위) → Right → Left
            pnlAlarmBanner.Controls.Add(lblBannerMsg);
            pnlAlarmBanner.Controls.Add(btnBannerReset);
            pnlAlarmBanner.Controls.Add(lblBannerIcon);

            this.Controls.Add(pnlAlarmBanner);
            // 헤더를 최상단 z-order 로 → 배너는 헤더 바로 아래, tabMain 이 나머지 채움
            this.Controls.SetChildIndex(this.headerPanel, this.Controls.Count - 1);
        }

        // timer1_Tick 에서 호출
        private void UpdateAlarmBanner()
        {
            bool show = alarm.HasAlarm;

            // 표시/숨김 전환 시에만 Visible 대입 (깜빡임 방지)
            if (show != _bannerVisible)
            {
                _bannerVisible = show;
                pnlAlarmBanner.Visible = show;
            }
            if (!show) return;

            string step = "-";
            var h = LogManager.Instance.AlarmHistory;
            if (h.Count > 0) step = h[h.Count - 1].Step;

            bool emg = equipment.IsEmergency;
            lblBannerMsg.Text = (emg ? "[EMG] " : "") + alarm.AlarmMessage + "     (Step: " + step + ")";

            // 배너 배경 점멸 + State 배지 점멸(빨강 ↔ 어두운 빨강)
            pnlAlarmBanner.BackColor = blinkFlag ? AlarmRedDark : AlarmRedLite;
            lblEquipState.ForeColor = blinkFlag ? Color.Red : Color.DarkRed;
        }
    }
}
