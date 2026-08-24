using System.Drawing;
using System.Windows.Forms;

namespace Ethercat_Test
{
    // ─────────────────────────────────────────────────────────────
    //  런타임 테마(코드) — Form1.Designer.cs 는 표준 형식 그대로 두고
    //  Form1_Load 에서 색상/폰트/플랫 스타일만 일괄 적용해 세련된 룩을 낸다.
    //  의미색이 지정된 컨트롤(타워 테스트 버튼/타워 인디케이터/슬롯)은 보존.
    // ─────────────────────────────────────────────────────────────
    public partial class Form1
    {
        private static readonly Color ThDark = Color.FromArgb(44, 62, 80);    // 헤더/텍스트
        private static readonly Color ThBg = Color.FromArgb(240, 242, 245);   // 배경
        private static readonly Color ThGreen = Color.FromArgb(39, 174, 96);
        private static readonly Color ThOrange = Color.FromArgb(230, 126, 34);
        private static readonly Color ThGray = Color.FromArgb(127, 140, 141);
        private static readonly Color ThMuted = Color.FromArgb(185, 195, 205);
        private static readonly Color ThBorder = Color.FromArgb(212, 217, 222);

        private void ApplyModernTheme()
        {
            this.Font = new Font("맑은 고딕", 9F, FontStyle.Regular);
            this.BackColor = ThBg;

            // ── 헤더 바 (다크 앱바) ──
            headerPanel.BackColor = ThDark;
            headerPanel.BorderStyle = BorderStyle.None;
            lblTitle.ForeColor = Color.White;
            lblSubtitle.ForeColor = ThMuted;
            lblClock.ForeColor = Color.White;
            label1.ForeColor = ThMuted;   // "Conn:"
            label2.ForeColor = Color.White;
            lblEquipState.BackColor = Color.White;   // 상태 배지: 흰 카드 + 동적 글자색(timer1_Tick)

            // ── 연결 / 액션 버튼 (강조색) ──
            StyleAccent(button1, ThGreen);       // Connection ON
            StyleAccent(button2, ThGray);        // Connection OFF
            StyleAccent(btnAutoStart, ThGreen);  // 자동공정시작
            StyleAccent(btnAutoStop, ThOrange);  // 자동공정정지
            StyleAccent(btnAlarmReset, ThGray);  // 알람리셋

            // ── 나머지 버튼 flat + 그룹박스 타이틀색 ──
            StyleTree(this);

            // ── 탭 배경 통일 ──
            tabOverview.BackColor = ThBg;
            tabManual.BackColor = ThBg;
        }

        private void StyleAccent(Button b, Color back)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(back, 0.15f);
            b.BackColor = back;
            b.ForeColor = Color.White;
            b.UseVisualStyleBackColor = false;
            b.Cursor = Cursors.Hand;
            b.Font = new Font("맑은 고딕", b.Font.Size, FontStyle.Bold);
        }

        private void StyleTree(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is Button b)
                {
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderColor = ThBorder;
                    b.Cursor = Cursors.Hand;
                    // 의미색이 없는(기본) 버튼만 흰 카드풍으로. accent/타워버튼은 색 보존.
                    if (b.UseVisualStyleBackColor)
                    {
                        b.BackColor = Color.White;
                        b.ForeColor = ThDark;
                        b.UseVisualStyleBackColor = false;
                    }
                }
                else if (c is GroupBox gb)
                {
                    gb.ForeColor = ThDark;
                }
                if (c.HasChildren) StyleTree(c);
            }
        }
    }
}
