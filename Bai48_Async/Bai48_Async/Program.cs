using System;
using System.Windows.Forms;

namespace StudentManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Vòng lặp: Đăng nhập -> FormMain -> (Đăng xuất thì quay lại Đăng nhập)
            while (true)
            {
                using var login = new FormLogin();
                if (login.ShowDialog() != DialogResult.OK) break;

                var main = new FormMain(login.UserName);
                Application.Run(main);

                if (!main.LoggedOut) break;   // Thoát hẳn
            }
        }
    }
}