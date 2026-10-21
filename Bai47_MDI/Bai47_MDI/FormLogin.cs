using System;
using System.Drawing;
using System.Windows.Forms;

namespace StudentManager
{
    public class FormLogin : Form
    {
        private TextBox txtUser = new TextBox();
        private TextBox txtPass = new TextBox();
        private Button btnLogin = new Button();
        private Button btnCancel = new Button();

        public string UserName => txtUser.Text.Trim();

        public FormLogin()
        {
            Text = "Đăng nhập";
            ClientSize = new Size(340, 190);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);

            Controls.Add(new Label { Text = "Tài khoản:", Left = 20, Top = 28, Width = 80 });
            txtUser.SetBounds(110, 25, 200, 28);
            txtUser.Text = "admin";

            Controls.Add(new Label { Text = "Mật khẩu:", Left = 20, Top = 68, Width = 80 });
            txtPass.SetBounds(110, 65, 200, 28);
            txtPass.UseSystemPasswordChar = true;

            Controls.Add(new Label
            {
                Text = "(Tài khoản bất kỳ, mật khẩu: 123)",
                Left = 20, Top = 102, Width = 300, ForeColor = Color.Gray
            });

            btnLogin.Text = "Đăng nhập";
            btnLogin.SetBounds(110, 135, 95, 34);
            btnLogin.Click += BtnLogin_Click;

            btnCancel.Text = "Thoát";
            btnCancel.SetBounds(215, 135, 95, 34);
            btnCancel.DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] { txtUser, txtPass, btnLogin, btnCancel });
            AcceptButton = btnLogin;
            CancelButton = btnCancel;
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text) || txtPass.Text != "123")
            {
                MessageBox.Show("Sai tài khoản hoặc mật khẩu!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult = DialogResult.OK;
        }
    }
}
