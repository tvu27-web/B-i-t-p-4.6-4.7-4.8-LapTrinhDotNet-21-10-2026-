using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace StudentManager
{
    public class FormProduct : Form, IChildForm
    {
        public event Action<string>? StatusMessage;
        public event Action<int>? ProgressChanged;
        public string CurrentUser { get; set; } = "";
        private void Notify(string msg) => StatusMessage?.Invoke("[" + CurrentUser + "] " + msg);

        private readonly BindingList<Product> _products = DataStore.Products;
        private readonly BindingSource _bs = new BindingSource();

        private DataGridView dgv = new DataGridView();
        private TextBox txtId = new TextBox();
        private TextBox txtName = new TextBox();
        private TextBox txtPrice = new TextBox();
        private TextBox txtQty = new TextBox();
        private Button btnAdd = new Button();
        private Button btnUpdate = new Button();
        private Button btnDelete = new Button();
        private GroupBox grpInput = new GroupBox();
        private ContextMenuStrip ctxGrid = new ContextMenuStrip();

        // ===== Bài 4.8: nạp dữ liệu bất đồng bộ =====
        private Panel pnlLoad = new Panel();
        private Button btnLoad = new Button();
        private ProgressBar progressBar = new ProgressBar();
        private Label lblPercent = new Label();
        private CheckBox chkError = new CheckBox();
        private CancellationTokenSource? _cts;

        public FormProduct()
        {
            BuildUI();
            BuildContextMenu();
            SetupBinding();
        }

        private Product? Current => _bs.Current as Product;

        private void BuildUI()
        {
            Text = "Quản lý Sản phẩm";
            Size = new Size(720, 480);
            Font = new Font("Segoe UI", 10);

            dgv.Dock = DockStyle.Fill;
            dgv.AutoGenerateColumns = false;
            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductId", HeaderText = "Mã SP", FillWeight = 20 });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Tên sản phẩm", FillWeight = 45 });
            var colPrice = new DataGridViewTextBoxColumn { DataPropertyName = "Price", HeaderText = "Đơn giá", FillWeight = 25 };
            colPrice.DefaultCellStyle.Format = "N0";
            dgv.Columns.Add(colPrice);
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Số lượng", FillWeight = 15 });
            dgv.DataError += (s, e) => e.ThrowException = false;
            dgv.CellMouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right && e.RowIndex >= 0)
                {
                    dgv.ClearSelection();
                    dgv.Rows[e.RowIndex].Selected = true;
                    dgv.CurrentCell = dgv.Rows[e.RowIndex].Cells[0];
                }
            };

            grpInput.Text = "Thông tin sản phẩm";
            grpInput.Dock = DockStyle.Top;
            grpInput.Height = 190;

            AddRow("Mã SP:", txtId, 30);
            AddRow("Tên SP:", txtName, 70);
            AddRow("Đơn giá:", txtPrice, 110);
            AddRow("Số lượng:", txtQty, 150);

            btnAdd.Text = "Thêm";
            btnUpdate.Text = "Cập nhật";
            btnDelete.Text = "Xóa";
            Button[] btns = { btnAdd, btnUpdate, btnDelete };
            for (int i = 0; i < btns.Length; i++)
            {
                btns[i].SetBounds(380, 30 + i * 48, 160, 38);
                grpInput.Controls.Add(btns[i]);
            }
            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += (s, e) => DeleteCurrent();

            // ----- Cụm nạp dữ liệu + ProgressBar (Bài 4.8) -----
            pnlLoad.Dock = DockStyle.Bottom;
            pnlLoad.Height = 46;
            btnLoad.Text = "Nạp dữ liệu";
            btnLoad.SetBounds(8, 7, 130, 32);
            btnLoad.Click += BtnLoad_Click;
            progressBar.SetBounds(150, 12, 270, 22);
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            lblPercent.SetBounds(430, 14, 50, 22);
            lblPercent.Text = "0%";
            chkError.Text = "Giả lập lỗi";
            chkError.SetBounds(490, 10, 120, 28);
            pnlLoad.Controls.AddRange(new Control[] { btnLoad, progressBar, lblPercent, chkError });

            Controls.Add(dgv);
            Controls.Add(pnlLoad);
            Controls.Add(grpInput);
        }

        private void AddRow(string label, TextBox tb, int top)
        {
            grpInput.Controls.Add(new Label { Text = label, Left = 15, Top = top + 3, Width = 80 });
            tb.SetBounds(100, top, 230, 28);
            grpInput.Controls.Add(tb);
        }

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
                "Mã SP: " + Current.ProductId + "\n" +
                "Tên SP: " + Current.ProductName + "\n" +
                "Đơn giá: " + Current.Price.ToString("N0") + " đ\n" +
                "Số lượng: " + Current.Quantity + "\n" +
                "Thành tiền tồn kho: " + (Current.Price * Current.Quantity).ToString("N0") + " đ",
                "Chi tiết sản phẩm", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SetupBinding()
        {
            _bs.DataSource = _products;
            dgv.DataSource = _bs;

            txtId.DataBindings.Add("Text", _bs, "ProductId", true, DataSourceUpdateMode.OnPropertyChanged);
            txtName.DataBindings.Add("Text", _bs, "ProductName", true, DataSourceUpdateMode.OnPropertyChanged);
            txtPrice.DataBindings.Add("Text", _bs, "Price", true, DataSourceUpdateMode.OnValidation);
            txtQty.DataBindings.Add("Text", _bs, "Quantity", true, DataSourceUpdateMode.OnValidation);

            _products.ListChanged += OnListChanged;
            UpdateEnabledState();
        }

        private void OnListChanged(object? sender, ListChangedEventArgs e) => UpdateEnabledState();

        private void UpdateEnabledState()
        {
            bool has = _products.Count > 0;
            txtId.Enabled = txtName.Enabled = txtPrice.Enabled = txtQty.Enabled = has;
            btnUpdate.Enabled = btnDelete.Enabled = has;
        }

        // IChildForm: Cha gọi xuống
        public void ReloadData()
        {
            _bs.ResetBindings(false);
            dgv.Refresh();
            Notify("Đã làm mới danh sách sản phẩm (" + _products.Count + " dòng).");
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            var p = new Product { ProductId = GenerateId(), Quantity = 1 };
            _products.Add(p);
            _bs.Position = _bs.IndexOf(p);
            txtName.Focus();
            Notify("Đã thêm sản phẩm mới " + p.ProductId + ". Hãy nhập tên và giá.");
        }

        private string GenerateId()
        {
            int n = _products.Count + 1;
            string id;
            do { id = "SP" + n.ToString("D3"); n++; }
            while (_products.Any(p => p.ProductId == id));
            return id;
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (Current == null) return;
            _bs.EndEdit();

            if (string.IsNullOrWhiteSpace(Current.ProductId) || string.IsNullOrWhiteSpace(Current.ProductName))
            {
                MessageBox.Show("Mã SP và Tên SP không được để trống!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (Current.Price < 0 || Current.Quantity < 0)
            {
                MessageBox.Show("Đơn giá và số lượng phải >= 0!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_products.Count(p => p.ProductId == Current.ProductId) > 1)
            {
                MessageBox.Show("Mã SP đã tồn tại!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _bs.ResetCurrentItem();
            dgv.Refresh();
            Notify("Đã cập nhật sản phẩm " + Current.ProductId + " thành công.");
        }

        private void DeleteCurrent()
        {
            if (Current == null) return;
            string id = Current.ProductId;
            if (MessageBox.Show("Xóa sản phẩm \"" + Current.ProductName + "\"?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            _bs.RemoveCurrent();
            Notify("Đã xóa sản phẩm " + id + ".");
        }

        // ------------------------------------------------------------
        // BÀI 4.8: NẠP DỮ LIỆU BẤT ĐỒNG BỘ + PROGRESSBAR
        // ------------------------------------------------------------
        // async void chỉ dùng cho event handler. Trong lúc "await", luồng UI được
        // trả lại cho Windows nên form không bị treo, vẫn kéo thả/bấm menu được.
        private async void BtnLoad_Click(object? sender, EventArgs e)
        {
            // Khóa nút để tránh bấm trùng lặp trong lúc đang nạp
            btnLoad.Enabled = false;
            chkError.Enabled = false;
            progressBar.Value = 0;
            lblPercent.Text = "0%";

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            // Progress<int> được tạo ở UI Thread nên callback luôn chạy ở UI Thread,
            // dù Report() được gọi từ luồng nền.
            var progress = new Progress<int>(p =>
            {
                if (IsDisposed) return;
                progressBar.Value = p;
                lblPercent.Text = p + "%";
                ProgressChanged?.Invoke(p);                                  // -> StatusStrip của cha
                StatusMessage?.Invoke("[" + CurrentUser + "] Đang nạp sản phẩm... " + p + "%");
            });

            try
            {
                Notify("Bắt đầu nạp danh sách sản phẩm...");

                List<Product> data = await DataService.LoadProductsAsync(progress, chkError.Checked, token);
                token.ThrowIfCancellationRequested();

                // Thay dữ liệu 1 lần rồi báo Reset (nhanh hơn Add từng dòng có thông báo)
                _products.RaiseListChangedEvents = false;
                _products.Clear();
                foreach (var item in data) _products.Add(item);
                _products.RaiseListChangedEvents = true;
                _products.ResetBindings();
                if (_products.Count > 0) _bs.Position = 0;

                Notify("Nạp xong " + _products.Count + " sản phẩm. Hoàn tất!");
                MessageBox.Show("Nạp dữ liệu hoàn tất: " + _products.Count + " sản phẩm.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                // Form bị đóng giữa chừng -> không làm gì thêm
            }
            catch (Exception ex)
            {
                if (IsDisposed) return;
                ProgressChanged?.Invoke(-1);          // ẩn thanh tiến trình ở form cha
                progressBar.Value = 0;
                lblPercent.Text = "0%";
                Notify("Lỗi khi nạp dữ liệu: " + ex.Message);
                MessageBox.Show("Lỗi khi nạp dữ liệu:\n" + ex.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Luôn mở lại nút, dù thành công hay lỗi
                if (!IsDisposed)
                {
                    btnLoad.Enabled = true;
                    chkError.Enabled = true;
                }
                _cts?.Dispose();
                _cts = null;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _cts?.Cancel();
            _products.ListChanged -= OnListChanged;
            _bs.Dispose();
            base.OnFormClosed(e);
        }
    }
}
