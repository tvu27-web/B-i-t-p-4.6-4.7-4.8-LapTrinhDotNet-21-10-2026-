using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace StudentManager
{
    public class Form1 : Form
    {
        // ===== Dữ liệu & BindingSource =====
        private readonly BindingList<Student> _students = new BindingList<Student>();
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

        public Form1()
        {
            BuildUI();
            SetupBinding();
            SeedData();
        }

        // ------------------------------------------------------------
        // 1. DỰNG GIAO DIỆN
        // ------------------------------------------------------------
        private void BuildUI()
        {
            Text = "Quản lý sinh viên - BindingSource & File";
            Size = new Size(860, 600);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);

            // DataGridView
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

            // Cụm nhập liệu
            grpInput.Text = "Thông tin sinh viên";
            grpInput.Dock = DockStyle.Top;
            grpInput.Height = 230;

            AddRow("Mã SV:", txtId, 30);
            AddRow("Họ tên:", txtName, 70);
            AddRow("Tuổi:", txtAge, 110);

            var lblGender = new Label { Text = "Giới tính:", Left = 15, Top = 153, Width = 80 };
            cboGender.DropDownStyle = ComboBoxStyle.DropDownList;
            cboGender.Items.AddRange(new object[] { "Nam", "Nữ" });
            cboGender.SetBounds(100, 150, 150, 28);
            grpInput.Controls.Add(lblGender);
            grpInput.Controls.Add(cboGender);

            picAvatar.SetBounds(300, 25, 150, 150);
            picAvatar.BorderStyle = BorderStyle.FixedSingle;
            picAvatar.SizeMode = PictureBoxSizeMode.Zoom;
            grpInput.Controls.Add(picAvatar);

            btnChooseImage.Text = "Chọn ảnh";
            btnChooseImage.SetBounds(300, 182, 150, 32);
            btnChooseImage.Click += BtnChooseImage_Click;
            grpInput.Controls.Add(btnChooseImage);

            // Cụm chức năng
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
            btnDelete.Click += BtnDelete_Click;
            btnExport.Click += BtnExport_Click;

            Controls.Add(dgv);      // Fill phải add trước
            Controls.Add(grpInput); // Top
        }

        private void AddRow(string label, TextBox tb, int top)
        {
            grpInput.Controls.Add(new Label { Text = label, Left = 15, Top = top + 3, Width = 80 });
            tb.SetBounds(100, top, 180, 28);
            grpInput.Controls.Add(tb);
        }

        // ------------------------------------------------------------
        // 2. CẤU HÌNH BINDINGSOURCE (2 CHIỀU)
        // ------------------------------------------------------------
        private void SetupBinding()
        {
            _bs.DataSource = _students;   // BindingList<Student> -> BindingSource
            dgv.DataSource = _bs;         // BindingSource -> DataGridView

            // BindingSource -> các control nhập liệu (2 chiều)
            // OnPropertyChanged: gõ đến đâu, dữ liệu + lưới cập nhật đến đó
            txtId.DataBindings.Add("Text", _bs, "StudentId", true, DataSourceUpdateMode.OnPropertyChanged);
            txtName.DataBindings.Add("Text", _bs, "FullName", true, DataSourceUpdateMode.OnPropertyChanged);
            txtAge.DataBindings.Add("Text", _bs, "Age", true, DataSourceUpdateMode.OnValidation);
            cboGender.DataBindings.Add("Text", _bs, "Gender", true, DataSourceUpdateMode.OnPropertyChanged);

            // Đổi dòng -> tự nạp ảnh tương ứng
            _bs.CurrentItemChanged += (s, e) => LoadAvatar();

            // Danh sách rỗng -> khóa cụm nhập liệu
            _students.ListChanged += (s, e) => UpdateEnabledState();
            UpdateEnabledState();
        }

        private void UpdateEnabledState()
        {
            bool has = _students.Count > 0;
            txtId.Enabled = txtName.Enabled = txtAge.Enabled = cboGender.Enabled = has;
            btnChooseImage.Enabled = btnUpdate.Enabled = btnDelete.Enabled = has;
            if (!has) LoadAvatar();
        }

        private void SeedData()
        {
            _students.Add(new Student { StudentId = "SV001", FullName = "Nguyễn Văn An", Age = 20, Gender = "Nam" });
            _students.Add(new Student { StudentId = "SV002", FullName = "Trần Thị Bình", Age = 19, Gender = "Nữ" });
            _students.Add(new Student { StudentId = "SV003", FullName = "Lê Hoàng Cường", Age = 21, Gender = "Nam" });
        }

        private Student? Current => _bs.Current as Student;

        // ------------------------------------------------------------
        // 3. HIỂN THỊ AVATAR
        // ------------------------------------------------------------
        private void LoadAvatar()
        {
            // Giải phóng ảnh cũ
            var old = picAvatar.Image;
            picAvatar.Image = null;
            old?.Dispose();

            string? path = Current?.AvatarPath;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;

            try
            {
                // Đọc qua stream rồi clone để không bị khóa file ảnh
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                using var img = Image.FromStream(fs);
                picAvatar.Image = new Bitmap(img);
            }
            catch
            {
                picAvatar.Image = null;
            }
        }

        // ------------------------------------------------------------
        // 4. CÁC NÚT CHỨC NĂNG
        // ------------------------------------------------------------
        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            // Tạo SV mới với mã tự sinh, các control đã binding sẽ tự hiện dòng mới
            var st = new Student
            {
                StudentId = GenerateId(),
                FullName = "",
                Age = 18,
                Gender = "Nam"
            };
            _students.Add(st);
            _bs.Position = _bs.IndexOf(st);   // chọn dòng vừa thêm
            txtName.Focus();
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
            _bs.EndEdit();   // đẩy dữ liệu từ control vào đối tượng

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
            MessageBox.Show("Cập nhật thành công!", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (Current == null) return;
            var rs = MessageBox.Show($"Xóa sinh viên \"{Current.FullName}\"?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (rs == DialogResult.Yes)
                _bs.RemoveCurrent();
        }

        // ------------------------------------------------------------
        // 5. THAO TÁC FILE
        // ------------------------------------------------------------
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
                Current.AvatarPath = ofd.FileName;  // gán vào SV đang chọn
                LoadAvatar();                       // hiển thị lên PictureBox
                dgv.Refresh();
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            if (_students.Count == 0)
            {
                MessageBox.Show("Danh sách trống, không có gì để xuất!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                // UTF-8 có BOM để Excel đọc đúng tiếng Việt
                using var sw = new StreamWriter(sfd.FileName, false, new UTF8Encoding(true));
                sw.WriteLine("StudentId,FullName,Age,Gender,AvatarPath");
                foreach (var s in _students)
                {
                    sw.WriteLine(string.Join(",",
                        Csv(s.StudentId), Csv(s.FullName), s.Age.ToString(),
                        Csv(s.Gender), Csv(s.AvatarPath)));
                }
                MessageBox.Show("Xuất file thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi ghi file: " + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Chuẩn CSV: bọc "..." nếu có dấu phẩy, nháy kép hoặc xuống dòng
        private static string Csv(string? value)
        {
            value ??= "";
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}
