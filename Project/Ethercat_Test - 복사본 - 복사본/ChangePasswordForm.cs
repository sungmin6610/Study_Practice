using System;
using System.Drawing;
using System.Windows.Forms;
using EtherCAT_Test.Auth;

namespace Ethercat_Test
{
    // ─────────────────────────────────────────────────────────────
    //  [이 파일이 하는 일]
    //  비밀번호를 바꾸는 작은 창. 두 상황에서 함께 쓰인다:
    //   ① 처음 로그인해서 반드시 바꿔야 할 때(forced=true, 닫기 버튼 숨김)
    //   ② 그냥 본인이 비번을 바꿀 때
    //  '새 비번'과 '확인'이 같은지, 4자 이상인지 검사한 뒤 UserManager 로 저장한다.
    // ─────────────────────────────────────────────────────────────
    /// <summary>비밀번호 변경 다이얼로그 (최초 로그인 강제 변경 / 본인 변경 공용).</summary>
    public class ChangePasswordForm : Form
    {
        private readonly string _username;
        private TextBox txtNew, txtConfirm;
        private Label lblMsg;

        public ChangePasswordForm(string username, bool forced)
        {
            _username = username;

            Text = "비밀번호 변경 - " + username;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ControlBox = !forced;   // 강제 변경이면 닫기(X) 숨김
            ClientSize = new Size(340, 200);
            Font = new Font("맑은 고딕", 9F);

            var head = new Label
            {
                Text = forced ? "최초 로그인 — 새 비밀번호를 설정하세요" : "새 비밀번호를 입력하세요",
                AutoSize = true,
                Location = new Point(20, 16),
                ForeColor = Color.MidnightBlue,
                Font = new Font("맑은 고딕", 10F, FontStyle.Bold)
            };
            var l1 = new Label { Text = "새 비밀번호", Location = new Point(24, 58), AutoSize = true };
            txtNew = new TextBox { Location = new Point(120, 54), Width = 190, UseSystemPasswordChar = true };
            var l2 = new Label { Text = "비밀번호 확인", Location = new Point(24, 92), AutoSize = true };
            txtConfirm = new TextBox { Location = new Point(120, 88), Width = 190, UseSystemPasswordChar = true };
            lblMsg = new Label { Location = new Point(24, 120), Size = new Size(300, 20), ForeColor = Color.Firebrick };

            var btnOk = new Button { Text = "변경", Location = new Point(120, 150), Size = new Size(90, 32) };
            var btnCancel = new Button { Text = "취소", Location = new Point(220, 150), Size = new Size(90, 32) };
            btnOk.Click += (s, e) => Apply();
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            AcceptButton = btnOk;
            CancelButton = btnCancel;

            Controls.AddRange(new Control[] { head, l1, txtNew, l2, txtConfirm, lblMsg, btnOk, btnCancel });
        }

        // '변경' 버튼을 눌렀을 때: 입력값을 검사하고 문제없으면 실제로 바꾼다.
        private void Apply()
        {
            string pw = txtNew.Text;
            if (pw.Length < 4)                    // 너무 짧으면 거부
            {
                lblMsg.Text = "비밀번호는 4자 이상이어야 합니다.";
                return;
            }
            if (pw != txtConfirm.Text)            // 두 칸이 서로 다르면 거부
            {
                lblMsg.Text = "비밀번호 확인이 일치하지 않습니다.";
                return;
            }
            if (!UserManager.Instance.ChangePassword(_username, pw))
            {
                lblMsg.Text = "변경 실패.";
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
