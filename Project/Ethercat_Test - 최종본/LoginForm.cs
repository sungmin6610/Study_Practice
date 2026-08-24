using System;
using System.Drawing;
using System.Windows.Forms;
using EtherCAT_Test.Auth;

namespace Ethercat_Test
{
    // ─────────────────────────────────────────────────────────────
    //  [이 파일이 하는 일]
    //  프로그램을 켜면 가장 먼저 뜨는 로그인 창.
    //  ': Form' = 윈도우 창 기능을 물려받는다(상속). 창 안의 글자상자/버튼은
    //  디자이너가 아니라 아래 생성자에서 코드로 직접 만들어 배치한다.
    //  로그인에 성공하면 창을 닫으며 결과를 'OK'로 남겨, Program.cs 가 메인 화면을 연다.
    // ─────────────────────────────────────────────────────────────
    /// <summary>로그인 화면. 인증 성공(및 필요 시 비밀번호 변경 완료) 시 DialogResult.OK.</summary>
    public class LoginForm : Form
    {
        private TextBox txtUser, txtPass;
        private Label lblMsg;

        public LoginForm()
        {
            Text = "로그인 - SEMI Photo Cluster";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(360, 236);
            Font = new Font("맑은 고딕", 9F);
            BackColor = Color.White;

            var lblT = new Label
            {
                Text = "SEMI Photo Cluster",
                Font = new Font("맑은 고딕", 14F, FontStyle.Bold),
                ForeColor = Color.MidnightBlue,
                AutoSize = true,
                Location = new Point(22, 18)
            };
            var lblSub = new Label { Text = "사용자 로그인", ForeColor = Color.Gray, AutoSize = true, Location = new Point(24, 48) };

            var l1 = new Label { Text = "사용자", Location = new Point(26, 84), AutoSize = true };
            txtUser = new TextBox { Location = new Point(110, 80), Width = 224 };
            var l2 = new Label { Text = "비밀번호", Location = new Point(26, 118), AutoSize = true };
            txtPass = new TextBox { Location = new Point(110, 114), Width = 224, UseSystemPasswordChar = true };

            lblMsg = new Label { Location = new Point(26, 146), Size = new Size(310, 20), ForeColor = Color.Firebrick };

            var btnLogin = new Button { Text = "로그인", Location = new Point(110, 176), Size = new Size(108, 34) };
            var btnCancel = new Button { Text = "취소", Location = new Point(226, 176), Size = new Size(108, 34) };
            btnLogin.Click += (s, e) => DoLogin();
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            txtPass.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) DoLogin(); };
            AcceptButton = btnLogin;
            CancelButton = btnCancel;

            Controls.AddRange(new Control[] { lblT, lblSub, l1, txtUser, l2, txtPass, lblMsg, btnLogin, btnCancel });
        }

        // 로그인 버튼/엔터를 눌렀을 때 실제 확인 절차.
        // UserManager 에게 아이디/비번을 확인시키고, 결과(res)에 따라 갈라진다.
        private void DoLogin()
        {
            var res = UserManager.Instance.TryLogin(txtUser.Text, txtPass.Text, out var user);
            if (res == AuthResult.Success)   // 성공
            {
                if (user.MustChangePassword)
                {
                    using (var cp = new ChangePasswordForm(user.Username, true))
                    {
                        if (cp.ShowDialog(this) != DialogResult.OK)
                        {
                            UserManager.Instance.Logout();   // 변경 취소 → 로그인 무효화
                            lblMsg.Text = "비밀번호 변경이 필요합니다.";
                            return;
                        }
                    }
                }
                DialogResult = DialogResult.OK;
                Close();
            }
            else if (res == AuthResult.Locked)
            {
                UserManager.Instance.IsLocked(txtUser.Text.Trim(), out int rem);
                lblMsg.Text = $"계정 잠금: {rem}초 후 다시 시도하세요.";
            }
            else
            {
                lblMsg.Text = "사용자 이름 또는 비밀번호가 올바르지 않습니다.";
                txtPass.SelectAll();
                txtPass.Focus();
            }
        }
    }
}
