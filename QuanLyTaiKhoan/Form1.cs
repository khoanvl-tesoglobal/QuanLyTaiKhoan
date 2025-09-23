using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace QuanLyTaiKhoan
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitGridAndControls();
            HookEvents();

            // Chỉ cho nhập số
            txtTien.KeyPress += TxtTien_KeyPress;
            txtSTK.KeyPress += TxtSTK_KeyPress;

            // Chỉ cho nhập chữ
            txtTenKH.KeyPress += TxtTenKH_KeyPress;
        }

        private void InitGridAndControls()
        {
            // Designer đã tạo sẵn các cột: colSTT, colMaTK, colTenKH, colDiaChi, colSoTien
            // Chỉ cấu hình hiển thị
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Textbox cấu hình
            txtTongTien.ReadOnly = true;
            txtTien.KeyPress += TxtTien_KeyPress; // chỉ cho nhập số

            // đảm bảo các textbox có thể nhập
            txtSTK.ReadOnly = false;
            txtSTK.Enabled = true;
            txtTenKH.ReadOnly = false;
            txtTenKH.Enabled = true;
            txtDiaChi.ReadOnly = false;
            txtDiaChi.Enabled = true;
        }

        private void HookEvents()
        {
            btnUpdate.Click += BtnUpdate_Click;
            btnClear.Click += BtnClear_Click; // nút Xóa
            btnExit.Click += BtnExit_Click;
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
        }

        private void TxtTien_KeyPress(object sender, KeyPressEventArgs e)
        {
            // chỉ cho phép nhập chữ số và phím điều khiển (ví dụ backspace)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        // Khi chọn hàng trên DataGridView -> đổ dữ liệu lên các TextBox
        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0) return;
            var row = dataGridView1.SelectedRows[0];
            txtSTK.Text = row.Cells["colMaTK"].Value?.ToString() ?? "";
            txtTenKH.Text = row.Cells["colTenKH"].Value?.ToString() ?? "";
            txtDiaChi.Text = row.Cells["colDiaChi"].Value?.ToString() ?? "";
            txtTien.Text = row.Cells["colSoTien"].Value?.ToString() ?? "0";
        }

        // Nút Thêm / Cập nhật
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            // Kiểm tra bắt buộc
            if (string.IsNullOrWhiteSpace(txtSTK.Text) ||
                string.IsNullOrWhiteSpace(txtTenKH.Text) ||
                string.IsNullOrWhiteSpace(txtDiaChi.Text) ||
                string.IsNullOrWhiteSpace(txtTien.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!long.TryParse(txtTien.Text.Trim(), out long tien) || tien < 0)
            {
                MessageBox.Show("Số tiền không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string maTK = txtSTK.Text.Trim();

            // Tìm row có mã tài khoản (colMaTK)
            var existingRow = FindRowByAccount(maTK);

            if (existingRow == null)
            {
                // Thêm mới: truyền đủ 5 giá trị tương ứng với 5 cột
                int nextSTT = dataGridView1.Rows.Count + 1;
                dataGridView1.Rows.Add(nextSTT, maTK, txtTenKH.Text.Trim(), txtDiaChi.Text.Trim(), tien.ToString());
                MessageBox.Show("Thêm mới dữ liệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Cập nhật
                existingRow.Cells["colTenKH"].Value = txtTenKH.Text.Trim();
                existingRow.Cells["colDiaChi"].Value = txtDiaChi.Text.Trim();
                existingRow.Cells["colSoTien"].Value = tien.ToString();
                MessageBox.Show("Cập nhật dữ liệu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            RecalculateSTT();
            RecalculateTotal();
            ClearInputFieldsAfterAddOrUpdate();
        }

        // Tìm row theo mã tài khoản (colMaTK)
        private DataGridViewRow FindRowByAccount(string account)
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var cell = row.Cells["colMaTK"].Value;
                if (cell != null && cell.ToString().Equals(account, StringComparison.OrdinalIgnoreCase))
                    return row;
            }
            return null;
        }

        // Nút Xóa (txtClear)
        private void BtnClear_Click(object sender, EventArgs e)
        {
            DataGridViewRow rowToDelete = null;
            if (dataGridView1.SelectedRows.Count > 0)
            {
                rowToDelete = dataGridView1.SelectedRows[0];
            }
            else if (!string.IsNullOrWhiteSpace(txtSTK.Text))
            {
                rowToDelete = FindRowByAccount(txtSTK.Text.Trim());
            }

            if (rowToDelete == null)
            {
                MessageBox.Show("Không tìm thấy số tài khoản cần xóa trong danh sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var res = MessageBox.Show("Bạn có chắc muốn xóa tài khoản này không?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                dataGridView1.Rows.Remove(rowToDelete);
                RecalculateSTT();
                RecalculateTotal();
                MessageBox.Show("Xóa tài khoản thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearInputFields();
            }
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void RecalculateSTT()
        {
            int i = 1;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.Cells["colSTT"].Value = i++;
            }
        }

        private void RecalculateTotal()
        {
            long total = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var val = row.Cells["colSoTien"].Value;
                if (val != null && long.TryParse(val.ToString(), out long amount))
                    total += amount;
            }
            txtTongTien.Text = total.ToString("N0", CultureInfo.InvariantCulture);
        }

        private void ClearInputFieldsAfterAddOrUpdate()
        {
            txtSTK.Clear();
            txtTenKH.Clear();
            txtDiaChi.Clear();
            txtTien.Text = "";
            txtSTK.Focus();
            dataGridView1.ClearSelection();
        }

        private void ClearInputFields()
        {
            txtSTK.Clear();
            txtTenKH.Clear();
            txtDiaChi.Clear();
            txtTien.Text = "";
            dataGridView1.ClearSelection();
        }

        private void TxtSTK_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Chỉ cho nhập số và phím điều khiển (Backspace...)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void TxtTenKH_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Chỉ cho nhập chữ, khoảng trắng và phím điều khiển
            if (!char.IsControl(e.KeyChar) && !char.IsLetter(e.KeyChar) && e.KeyChar != ' ')
            {
                e.Handled = true;
            }
        }
    }
}
