using System;
using System.Drawing;
using System.Windows.Forms;
using EtherCAT_Test.Auth;

namespace Ethercat_Test
{
    // ─────────────────────────────────────────────────────────────
    //  로그인/권한 Form1 통합 (partial). 헤더 사용자/로그아웃 + 권한 헬퍼.
    // ─────────────────────────────────────────────────────────────
    public partial class Form1
    {
        private Label lblUser;
        private Button btnLogout;
        private ToolTip permTip;

        private void InitAuthUI()
        {
            permTip = new ToolTip { ShowAlways = true };

            lblUser = new Label
            {
                AutoSize = true,
                ForeColor = Color.White,
                Font = new Font("맑은 고딕", 9F, FontStyle.Bold),
                Location = new Point(896, 12),
                Text = ""
            };
            btnLogout = new Button
            {
                Text = "로그아웃",
                Location = new Point(896, 32),
                Size = new Size(100, 24)
            };
            btnLogout.Click += (s, e) => DoLogout();

            headerPanel.Controls.Add(lblUser);
            headerPanel.Controls.Add(btnLogout);
            lblUser.BringToFront();
            btnLogout.BringToFront();

            UpdateUserHeader();
        }

        private void UpdateUserHeader()
        {
            var u = UserManager.Instance.CurrentUser;
            if (lblUser != null)
                lblUser.Text = u != null ? $"● {u.Username}  ({u.Role})" : "● (로그아웃됨)";
        }

        private void DoLogout()
        {
            UserManager.Instance.Logout();
            UpdateUserHeader();

            this.Hide();
            using (var login = new LoginForm())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    UpdateUserHeader();
                    UpdateControlEnable();
                    RefreshUserList();     // 관리자 탭 목록 갱신
                    this.Show();
                }
                else
                {
                    Application.Exit();    // 재로그인 취소 → 종료
                }
            }
        }

        /// <summary>권한이 필요한 조작 직전 재확인. 부족하면 MessageBox 후 false.</summary>
        private bool RequireRole(UserRole r)
        {
            var u = UserManager.Instance.CurrentUser;
            if (u == null || !u.HasPermission(r))
            {
                MessageBox.Show($"권한 부족: {r} 이상 권한이 필요합니다.", "권한 부족",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }
}
