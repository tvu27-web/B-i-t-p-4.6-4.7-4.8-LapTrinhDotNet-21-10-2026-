using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StudentManager
{
    public class FormStudent : Form, IChildForm
    {
        // ===== Giao tiếp với Form cha =====
        public event Action<string>? StatusMessage;
        public string CurrentUser { get; set; } = "";

        private void Notify(string msg) => StatusMessage?.Invoke("[" + CurrentUser + "] " + msg);

        // ===== Dữ liệu =====
        private readonly BindingList<Student> _students = DataStore.Students;
        private readonly BindingSource _bs = new BindingSource();

        // ===== Controls =====
        private DataGridView dgv = new DataGridView();
        private TextBox txtId = new TextBox();
        private TextBox txtName = new TextBox();
        private TextBox txtAge = new TextBox();
        private ComboBox cboGender = new ComboBox();
        private PictureBox picAvatar = new PictureBox();
        private Button btnChooseImage = new Button();
        private Button btnAdd = new Button();
        private Button btnDelete = new Button();
        private Button btnUpdate = new Button();
        private Button btnExport = new Button();
        private GroupBox grpInput = new GroupBox();
        private ContextMenuStrip ctxGrid = new ContextMenuStrip();

        public FormStudent()
        {
            BuildUI();
            BuildContextMenu();
            SetupBinding();
        }

        private Student? Current => _bs.Current as Student;

        // ------------------------------------------------------------
        private void BuildUI()
        {
            Text = "Quản lý Sinh viên";
            Size = new Size(820, 560);
            Font = new Font("Segoe UI", 10);

            dgv.Dock = DockStyle.Fill;
            dgv.AutoGenerateColumns = false;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StudentId", HeaderText = "Mã SV", FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FullName", HeaderText = "Họ tên", FillWeight = 40 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Age", HeaderText = "Tuổi", FillWeight = 10 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Gender", HeaderText = "Giới tính", FillWeight = 15 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AvatarPath", HeaderText = "Ảnh", FillWeight = 40 });
            dgv.DataError += (s, e) => e.ThrowException = false;

            // Bấm chuột phải vào dòng nào thì chọn dòng đó trước khi hiện menu
            dgv.CellMouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
                {
                    dgv.ClearSelection();
                    dgv.Rows[e.RowIndex].Selected = true;
                    dgv.CurrentCell = dgv.Rows[e.RowIndex].Cells[0];
                }
            };

            grpInput.Text = "Thông tin sinh viên";
            grpInput.Dock = DockStyle.Top;
            grpInput.Height = 230;

            AddRow("Mã SV:", txtId, 30);
            AddRow("Họ tên:", txtName, 70);
            AddRow("Tuổi:", txtAge, 110);

            grpInput.Controls.Add(new Label { Text = "Giới tính:", Left = 15, Top = 153, Width = 80 });
            cboGender.DropDownStyle = ComboBoxStyle.DropDownList;
            cboGender.Items.AddRange(new object[] { "Nam", "Nữ" });
            cboGender.SetBounds(100, 150, 150, 28);
            grpInput.Controls.Add(cboGender);

            picAvatar.SetBounds(300, 25, 150, 150);
            picAvatar.BorderStyle = BorderStyle.FixedSingle;
            picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            grpInput.Controls.Add(picAvatar);

            btnChooseImage.Text = "Chọn ảnh";
            btnChooseImage.SetBounds(300, 182, 150, 32);
            btnChooseImage.Click += BtnChooseImage_Click;
            grpInput.Controls.Add(btnChooseImage);

            btnAdd.Text = "Thêm";
            btnUpdate.Text = "Cập nhật";
            btnDelete.Text = "Xóa";
            btnExport.Text = "Xuất file CSV";
            Button[] btns = { btnAdd, btnUpdate, btnDelete, btnExport };
            for (int i = 0; i < btns.Length; i++)
            {
                btns[i].SetBounds(500, 25 + i * 48, 160, 38);
                grpInput.Controls.Add(btns[i]);
            }
            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += (s, e) => DeleteCurrent();
            btnExport.Click += BtnExport_Click;

            Controls.Add(dgv);
            Controls.Add(grpInput);
        }

        private void AddRow(string label, TextBox tb, int top)
        {
            grpInput.Controls.Add(new Label { Text = label, Left = 15, Top = top + 3, Width = 80 });
            tb.SetBounds(100, top, 180, 28);
            grpInput.Controls.Add(tb);
        }

        // ------------------------------------------------------------
        // CONTEXTMENUSTRIP cho DataGridView
        // ------------------------------------------------------------
        private void BuildContextMenu()
        {
            ctxGrid.Items.Add("Xem chi tiết", null, (s, e) => ShowDetail());
            ctxGrid.Items.Add("Làm mới dữ liệu", null, (s, e) => ReloadData());
            ctxGrid.Items.Add(new ToolStripSeparator());
            ctxGrid.Items.Add("Xóa dòng", null, (s, e) => DeleteCurrent());
            dgv.ContextMenuStrip = ctxGrid;
        }

        private void ShowDetail()
        {
            if (Current == null) return;
            MessageBox.Show(
                "Mã SV: " + Current.StudentId + "\n" +
                "Họ tên: " + Current.FullName + "\n" +
                "Tuổi: " + Current.Age + "\n" +
                "Giới tính: " + Current.Gender + "\n" +
                "Ảnh: " + (string.IsNullOrEmpty(Current.AvatarPath) ? "(chưa có)" : Current.AvatarPath),
                "Chi tiết sinh viên", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ------------------------------------------------------------
        // BINDING
        // ------------------------------------------------------------
        private void SetupBinding()
        {
            _bs.DataSource = _students;
            dgv.DataSource = _bs;

            txtId.DataBindings.Add("Text", _bs, "StudentId", true, DataSourceUpdateMode.OnPropertyChanged);
            txtName.DataBindings.Add("Text", _bs, "FullName", true, DataSourceUpdateMode.OnPropertyChanged);
            txtAge.DataBindings.Add("Text", _bs, "Age", true, DataSourceUpdateMode.OnValidation);
            cboGender.DataBindings.Add("Text", _bs, "Gender", true, DataSourceUpdateMode.OnPropertyChanged);

            _bs.CurrentItemChanged += (s, e) => LoadAvatar();
            _students.ListChanged += OnListChanged;
            UpdateEnabledState();
            LoadAvatar();
        }

        private void OnListChanged(object? sender, ListChangedEventArgs e) => UpdateEnabledState();

        private void UpdateEnabledState()
        {
            bool has = _students.Count > 0;
            txtId.Enabled = txtName.Enabled = txtAge.Enabled = cboGender.Enabled = has;
            btnChooseImage.Enabled = btnUpdate.Enabled = btnDelete.Enabled = has;
            if (!has) LoadAvatar();
        }

        private void LoadAvatar()
        {
            var old = picAvatar.Image;
            picAvatar.Image = null;
            old?.Dispose();

            string? path = Current?.AvatarPath;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                using var img = Image.FromStream(fs);
                picAvatar.Image = new Bitmap(img);
            }
            catch { picAvatar.Image = null; }
        }

        // ------------------------------------------------------------
        // IChildForm: Cha gọi xuống
        // ------------------------------------------------------------
        public void ReloadData()
        {
            _bs.ResetBindings(false);
            dgv.Refresh();
            LoadAvatar();
            Notify("Đã làm mới danh sách sinh viên (" + _students.Count + " dòng).");
        }

        // ------------------------------------------------------------
        // CÁC NÚT
        // ------------------------------------------------------------
        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var st = new Student { StudentId = GenerateId(), Age = 18, Gender = "Nam" };
            _students.Add(st);
            _bs.Position = _bs.IndexOf(st);
            txtName.Focus();
            Notify("Đã thêm sinh viên mới " + st.StudentId + ". Hãy nhập họ tên.");
        }

        private string GenerateId()
        {
            int n = _students.Count + 1;
            string id;
            do { id = "SV" + n.ToString("D3"); n++; }
            while (_students.Any(s => s.StudentId == id));
            return id;
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (Current == null) return;
            _bs.EndEdit();

            if (string.IsNullOrWhiteSpace(Current.StudentId) || string.IsNullOrWhiteSpace(Current.FullName))
            {
                MessageBox.Show("Mã SV và Họ tên không được để trống!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (Current.Age <= 0 || Current.Age > 100)
            {
                MessageBox.Show("Tuổi không hợp lệ (1 - 100)!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_students.Count(s => s.StudentId == Current.StudentId) > 1)
            {
                MessageBox.Show("Mã SV đã tồn tại!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _bs.ResetCurrentItem();
            dgv.Refresh();
            Notify("Đã cập nhật sinh viên " + Current.StudentId + " thành công.");
        }

        private void DeleteCurrent()
        {
            if (Current == null) return;
            string id = Current.StudentId;
            if (MessageBox.Show("Xóa sinh viên \"" + Current.FullName + "\"?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            _bs.RemoveCurrent();
            Notify("Đã xóa sinh viên " + id + ".");
        }

        private void BtnChooseImage_Click(object? sender, EventArgs e)
        {
            if (Current == null) return;
            using var ofd = new OpenFileDialog
            {
                Title = "Chọn ảnh đại diện",
                Filter = "Ảnh (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                Current.AvatarPath = ofd.FileName;
                LoadAvatar();
                dgv.Refresh();
                Notify("Đã đổi ảnh đại diện cho " + Current.StudentId + ".");
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            if (_students.Count == 0)
            {
                MessageBox.Show("Danh sách trống!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using var sfd = new SaveFileDialog
            {
                Title = "Xuất danh sách sinh viên",
                Filter = "CSV (*.csv)|*.csv",
                FileName = "DanhSachSinhVien.csv"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                using var sw = new StreamWriter(sfd.FileName, false, new UTF8Encoding(true));
                sw.WriteLine("StudentId,FullName,Age,Gender,AvatarPath");
                foreach (var s in _students)
                    sw.WriteLine(string.Join(",", Csv(s.StudentId), Csv(s.FullName), s.Age,
                        Csv(s.Gender), Csv(s.AvatarPath)));
                Notify("Đã xuất " + _students.Count + " sinh viên ra file CSV.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi ghi file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string Csv(string? v)
        {
            v ??= "";
            if (v.Contains(',') || v.Contains('"') || v.Contains('\n') || v.Contains('\r'))
                return "\"" + v.Replace("\"", "\"\"") + "\"";
            return v;
        }

        // Danh sách dùng chung (static) nên phải ngắt kết nối khi form đóng
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _students.ListChanged -= OnListChanged;
            _bs.Dispose();
            picAvatar.Image?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
