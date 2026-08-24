using System;
using System.Drawing;
using System.Windows.Forms;
using EtherCAT_Test.Auth;

namespace Ethercat_Test
{
    // ─────────────────────────────────────────────────────────────
    //  관리자 화면 (Administrator 만 조작 가능). 계정 CRUD/역할/비번초기화.
    // ─────────────────────────────────────────────────────────────
    public partial class Form1
    {
        private TabPage tabAdmin;
        private Panel pnlAdmin;
        private ListView lvUsers;
        private ComboBox cboRole;
        private TextBox txtNewUser, txtNewPass;

        private void InitAdminTab()
        {
            tabAdmin = new TabPage("관리자") { BackColor = Color.White };
            pnlAdmin = new Panel { Dock = DockStyle.Fill };

            lvUsers = new ListView
            {
                Location = new Point(20, 20),
                Size = new Size(560, 380),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                Font = new Font("맑은 고딕", 10F)
            };
            lvUsers.Columns.Add("사용자", 200);
            lvUsers.Columns.Add("권한", 160);
            lvUsers.Columns.Add("비밀번호 변경 필요", 180);

            int rx = 610;
            var lblNew = new Label { Text = "신규 계정 추가", Location = new Point(rx, 20), AutoSize = true, Font = new Font("맑은 고딕", 10F, FontStyle.Bold) };
            var lblU = new Label { Text = "ID", Location = new Point(rx, 54), AutoSize = true };
            txtNewUser = new TextBox { Location = new Point(rx + 60, 50), Width = 200 };
            var lblP = new Label { Text = "PW", Location = new Point(rx, 88), AutoSize = true };
            txtNewPass = new TextBox { Location = new Point(rx + 60, 84), Width = 200, UseSystemPasswordChar = true };
            var lblR = new Label { Text = "역할", Location = new Point(rx, 122), AutoSize = true };
            cboRole = new ComboBox { Location = new Point(rx + 60, 118), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cboRole.Items.AddRange(new object[] { UserRole.Operator, UserRole.Engineer, UserRole.Administrator });
            cboRole.SelectedIndex = 0;

            var btnAdd = new Button { Text = "계정 추가", Location = new Point(rx, 158), Size = new Size(260, 32) };
            var lblSel = new Label { Text = "선택 계정 관리", Location = new Point(rx, 210), AutoSize = true, Font = new Font("맑은 고딕", 10F, FontStyle.Bold) };
            var btnRole = new Button { Text = "역할 변경(선택 → 위 역할)", Location = new Point(rx, 238), Size = new Size(260, 32) };
            var btnReset = new Button { Text = "비밀번호 초기화(선택 → 위 PW)", Location = new Point(rx, 274), Size = new Size(260, 32) };
            var btnDel = new Button { Text = "선택 계정 삭제", Location = new Point(rx, 310), Size = new Size(260, 32) };

            btnAdd.Click += (s, e) => AdminAdd();
            btnRole.Click += (s, e) => AdminChangeRole();
            btnReset.Click += (s, e) => AdminResetPw();
            btnDel.Click += (s, e) => AdminDelete();

            pnlAdmin.Controls.AddRange(new Control[]
            {
                lvUsers, lblNew, lblU, txtNewUser, lblP, txtNewPass, lblR, cboRole,
                btnAdd, lblSel, btnRole, btnReset, btnDel
            });
            tabAdmin.Controls.Add(pnlAdmin);
            tabMain.TabPages.Add(tabAdmin);

            RefreshUserList();
        }

        // UpdateControlEnable 에서 권한(Administrator)으로 토글
        private void SetAdminEnabled(bool en)
        {
            if (pnlAdmin != null) SetEnabled(pnlAdmin, en);
        }

        private void RefreshUserList()
        {
            if (lvUsers == null) return;
            lvUsers.BeginUpdate();
            lvUsers.Items.Clear();
            foreach (var u in UserManager.Instance.Users)
            {
                var it = new ListViewItem(u.Username);
                it.SubItems.Add(u.Role.ToString());
                it.SubItems.Add(u.MustChangePassword ? "예" : "-");
                lvUsers.Items.Add(it);
            }
            lvUsers.EndUpdate();
        }

        private string SelectedUser()
            => (lvUsers != null && lvUsers.SelectedItems.Count > 0) ? lvUsers.SelectedItems[0].Text : null;

        private void AdminAdd()
        {
            if (!RequireRole(UserRole.Administrator)) return;
            string id = txtNewUser.Text.Trim();
            if (id.Length == 0 || txtNewPass.Text.Length < 4)
            {
                MessageBox.Show("ID와 4자 이상 비밀번호를 입력하세요.");
                return;
            }
            var role = (UserRole)cboRole.SelectedItem;
            if (UserManager.Instance.AddUser(id, txtNewPass.Text, role))
            {
                txtNewUser.Clear();
                txtNewPass.Clear();
                RefreshUserList();
            }
            else MessageBox.Show("추가 실패 (중복 ID 등).");
        }

        private void AdminDelete()
        {
            if (!RequireRole(UserRole.Administrator)) return;
            string id = SelectedUser();
            if (id == null) return;
            if (MessageBox.Show($"'{id}' 계정을 삭제할까요?", "확인",
                MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            if (UserManager.Instance.DeleteUser(id)) RefreshUserList();
            else MessageBox.Show("삭제 실패 (본인/마지막 관리자 보호).");
        }

        private void AdminChangeRole()
        {
            if (!RequireRole(UserRole.Administrator)) return;
            string id = SelectedUser();
            if (id == null) return;
            var role = (UserRole)cboRole.SelectedItem;
            if (UserManager.Instance.ChangeRole(id, role)) RefreshUserList();
            else MessageBox.Show("역할 변경 실패 (마지막 관리자 강등 불가).");
        }

        private void AdminResetPw()
        {
            if (!RequireRole(UserRole.Administrator)) return;
            string id = SelectedUser();
            if (id == null) return;
            if (txtNewPass.Text.Length < 4)
            {
                MessageBox.Show("초기화할 비밀번호(4자 이상)를 우측 PW 입력란에 넣으세요.");
                return;
            }
            if (UserManager.Instance.ResetPassword(id, txtNewPass.Text))
            {
                txtNewPass.Clear();
                RefreshUserList();
                MessageBox.Show($"'{id}' 비밀번호 초기화 완료 (최초 로그인 시 변경 요구).");
            }
        }
    }
}
