using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace StudentManager
{
    public class FormMain : Form
    {
        private readonly string _userName;

        private MenuStrip menuStrip = new MenuStrip();
        private ToolStrip toolStrip = new ToolStrip();
        private StatusStrip statusStrip = new StatusStrip();
        private ContextMenuStrip ctxMain = new ContextMenuStrip();

        private ToolStripStatusLabel lblUser = new ToolStripStatusLabel();
        private ToolStripStatusLabel lblMessage = new ToolStripStatusLabel();
        private ToolStripStatusLabel lblTime = new ToolStripStatusLabel();
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        // Cho Program.cs biết là đăng xuất (quay về Login) hay thoát hẳn
        public bool LoggedOut { get; private set; }

        public FormMain(string userName)
        {
            _userName = userName;

            Text = "Quản lý - MDI";
            Size = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);
            IsMdiContainer = true;                 // <-- biến Form thành MDI Parent

            BuildMenu();
            BuildToolStrip();
            BuildStatusStrip();
            BuildContextMenu();

            // Thứ tự add: Status -> Tool -> Menu để Menu nằm trên cùng
            Controls.Add(statusStrip);
            Controls.Add(toolStrip);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;

            // ContextMenuStrip của cha: gắn vào vùng nền MDI (MdiClient)
            foreach (Control c in Controls)
                if (c is MdiClient) c.ContextMenuStrip = ctxMain;

            SetMessage("Sẵn sàng.");
        }

        // ------------------------------------------------------------
        // MENU
        // ------------------------------------------------------------
        private void BuildMenu()
        {
            // --- Hệ thống ---
            var mnuSystem = new ToolStripMenuItem("Hệ &thống");
            mnuSystem.DropDownItems.Add(new ToolStripMenuItem("Đăng xuất", null, (s, e) => Logout())
            { ShortcutKeys = Keys.Control | Keys.L });
            mnuSystem.DropDownItems.Add(new ToolStripSeparator());
            mnuSystem.DropDownItems.Add(new ToolStripMenuItem("Thoát", null, (s, e) => ExitApp())
            { ShortcutKeys = Keys.Control | Keys.Q });

            // --- Danh mục ---
            var mnuCategory = new ToolStripMenuItem("&Danh mục");
            mnuCategory.DropDownItems.Add(new ToolStripMenuItem("Quản lý Sinh viên", null,
                (s, e) => OpenChild<FormStudent>()) { ShortcutKeys = Keys.Control | Keys.D1 });
            mnuCategory.DropDownItems.Add(new ToolStripMenuItem("Quản lý Sản phẩm", null,
                (s, e) => OpenChild<FormProduct>()) { ShortcutKeys = Keys.Control | Keys.D2 });

            // --- Cửa sổ ---
            var mnuWindow = new ToolStripMenuItem("&Cửa sổ");
            mnuWindow.DropDownItems.Add(new ToolStripMenuItem("Cascade", null,
                (s, e) => LayoutMdi(MdiLayout.Cascade)));
            mnuWindow.DropDownItems.Add(new ToolStripMenuItem("Tile Horizontal", null,
                (s, e) => LayoutMdi(MdiLayout.TileHorizontal)));
            mnuWindow.DropDownItems.Add(new ToolStripMenuItem("Tile Vertical", null,
                (s, e) => LayoutMdi(MdiLayout.TileVertical)));
            mnuWindow.DropDownItems.Add(new ToolStripSeparator());
            mnuWindow.DropDownItems.Add(new ToolStripMenuItem("Đóng tất cả", null, (s, e) => CloseAllChildren()));

            menuStrip.Items.AddRange(new ToolStripItem[] { mnuSystem, mnuCategory, mnuWindow });

            // Tự liệt kê các cửa sổ con đang mở ở cuối menu Cửa sổ
            menuStrip.MdiWindowListItem = mnuWindow;
        }

        // ------------------------------------------------------------
        // TOOLSTRIP
        // ------------------------------------------------------------
        private void BuildToolStrip()
        {
            toolStrip.ImageScalingSize = new Size(32, 32);

            toolStrip.Items.Add(MakeButton("Sinh viên", "S", Color.SteelBlue, "Mở Quản lý Sinh viên",
                (s, e) => OpenChild<FormStudent>()));
            toolStrip.Items.Add(MakeButton("Sản phẩm", "P", Color.SeaGreen, "Mở Quản lý Sản phẩm",
                (s, e) => OpenChild<FormProduct>()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(MakeButton("Cascade", "C", Color.DarkOrange, "Sắp xếp xếp tầng",
                (s, e) => LayoutMdi(MdiLayout.Cascade)));
            toolStrip.Items.Add(MakeButton("Ngang", "H", Color.MediumPurple, "Tile Horizontal",
                (s, e) => LayoutMdi(MdiLayout.TileHorizontal)));
            toolStrip.Items.Add(MakeButton("Dọc", "V", Color.Teal, "Tile Vertical",
                (s, e) => LayoutMdi(MdiLayout.TileVertical)));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(MakeButton("Làm mới", "R", Color.Crimson, "Làm mới dữ liệu cửa sổ đang chọn",
                (s, e) => RefreshActiveChild()));
            toolStrip.Items.Add(MakeButton("Thoát", "X", Color.Gray, "Thoát chương trình",
                (s, e) => ExitApp()));
        }

        private ToolStripButton MakeButton(string text, string letter, Color color, string tip, EventHandler onClick)
        {
            return new ToolStripButton(text, IconHelper.Make(letter, color), onClick)
            {
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                TextImageRelation = TextImageRelation.ImageAboveText,
                ToolTipText = tip
            };
        }

        // ------------------------------------------------------------
        // STATUSSTRIP
        // ------------------------------------------------------------
        private void BuildStatusStrip()
        {
            lblUser.Text = "Người dùng: " + _userName;
            lblUser.BorderSides = ToolStripStatusLabelBorderSides.Right;

            lblMessage.Spring = true;                    // chiếm phần trống ở giữa
            lblMessage.TextAlign = ContentAlignment.MiddleLeft;

            lblTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            statusStrip.Items.AddRange(new ToolStripItem[] { lblUser, lblMessage, lblTime });

            timer.Interval = 1000;
            timer.Tick += (s, e) => lblTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            timer.Start();
        }

        // ------------------------------------------------------------
        // CONTEXTMENUSTRIP (của cha, bấm chuột phải vào nền MDI)
        // ------------------------------------------------------------
        private void BuildContextMenu()
        {
            ctxMain.Items.Add("Mở Quản lý Sinh viên", null, (s, e) => OpenChild<FormStudent>());
            ctxMain.Items.Add("Mở Quản lý Sản phẩm", null, (s, e) => OpenChild<FormProduct>());
            ctxMain.Items.Add(new ToolStripSeparator());
            ctxMain.Items.Add("Cascade", null, (s, e) => LayoutMdi(MdiLayout.Cascade));
            ctxMain.Items.Add("Tile Horizontal", null, (s, e) => LayoutMdi(MdiLayout.TileHorizontal));
            ctxMain.Items.Add("Tile Vertical", null, (s, e) => LayoutMdi(MdiLayout.TileVertical));
        }

        // ------------------------------------------------------------
        // THUẬT TOÁN MỞ FORM CON: chưa mở -> tạo mới; đã mở -> Activate
        // ------------------------------------------------------------
        private void OpenChild<T>() where T : Form, IChildForm, new()
        {
            // Tìm trong danh sách form con xem đã có form kiểu T chưa
            var existing = MdiChildren.OfType<T>().FirstOrDefault();

            if (existing != null)
            {
                if (existing.WindowState == FormWindowState.Minimized)
                    existing.WindowState = FormWindowState.Normal;
                existing.Activate();                 // đưa lên trên cùng
                SetMessage("Cửa sổ \"" + existing.Text + "\" đã mở, đã kích hoạt.");
                return;
            }

            var child = new T();
            child.MdiParent = this;                  // nhúng vào form cha
            child.CurrentUser = _userName;           // Cha -> Con: truyền dữ liệu
            child.StatusMessage += SetMessage;       // Con -> Cha: nhận thông điệp
            child.FormClosed += (s, e) =>
            {
                child.StatusMessage -= SetMessage;
                SetMessage("Đã đóng cửa sổ \"" + child.Text + "\".");
            };
            child.Show();
            SetMessage("Đã mở cửa sổ \"" + child.Text + "\".");
        }

        // ------------------------------------------------------------
        // CÁC HÀM DÙNG CHUNG
        // ------------------------------------------------------------
        // Hàm này được Form con gọi (thông qua event) để đổi chữ trên StatusStrip
        public void SetMessage(string message)
        {
            lblMessage.Text = DateTime.Now.ToString("HH:mm:ss") + " - " + message;
        }

        private void RefreshActiveChild()
        {
            if (ActiveMdiChild is IChildForm child)
                child.ReloadData();                  // Cha gọi hàm của Con
            else
                SetMessage("Chưa có cửa sổ con nào đang mở.");
        }

        private void CloseAllChildren()
        {
            foreach (var f in MdiChildren) f.Close();
        }

        private void Logout()
        {
            if (MessageBox.Show("Bạn có muốn đăng xuất?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            LoggedOut = true;
            Close();
        }

        private void ExitApp()
        {
            if (MessageBox.Show("Bạn có muốn thoát chương trình?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            LoggedOut = false;
            Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            timer.Stop();
            timer.Dispose();
            base.OnFormClosed(e);
        }
    }
}
