using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClinicApp.BLL;
using ClinicApp.DTO;

namespace ClinicApp.GUI.Forms;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class FrmBenhNhan : Form // Use Form instead of PlaceholderForm if we design it fully

{
    private readonly BenhNhanBLL _benhNhanBLL = new();
    
    // UI Controls
    private TextBox txtSearch;
    private Button btnSearch;
    private DataGridView dgvBenhNhan;
    
    private TextBox txtHoTen;
    private DateTimePicker dtpNgaySinh;
    private CheckBox chkNgaySinh;
    private ComboBox cmbGioiTinh;
    private TextBox txtSDT;
    private TextBox txtCCCD;
    private TextBox txtDiaChi;
    
    private Button btnThem;
    private Button btnSua;
    private Button btnLamMoi;
    
    private int _currentMaBN = 0;

    public FrmBenhNhan()
    {
        InitializeComponent();
        SetupTheme();
        Load += (_, _) => LoadData();
    }

    private void InitializeComponent()
    {
        this.Text = "Quản lý bệnh nhân";
        this.Size = new Size(1000, 600);
        this.StartPosition = FormStartPosition.CenterParent;

        // Top Panel for Search
        var pnlTop = new Panel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(10) };
        var lblSearch = new Label { Text = "Tìm kiếm:", AutoSize = true, Location = new Point(10, 20) };
        txtSearch = new TextBox { Location = new Point(80, 17), Width = 300, PlaceholderText = "Tên, SĐT, CCCD..." };
        btnSearch = new Button { Text = "Tìm", Location = new Point(390, 15), Width = 80, BackColor = Color.FromArgb(0, 85, 150), ForeColor = Color.White };
        btnSearch.Click += BtnSearch_Click;
        pnlTop.Controls.AddRange(new Control[] { lblSearch, txtSearch, btnSearch });

        // Right Panel for Form
        var pnlRight = new Panel { Dock = DockStyle.Right, Width = 350, Padding = new Padding(10) };
        
        int y = 20;
        int spacing = 40;
        
        pnlRight.Controls.Add(new Label { Text = "Họ Tên (*):", Location = new Point(10, y), AutoSize = true });
        txtHoTen = new TextBox { Location = new Point(100, y), Width = 230 };
        y += spacing;
        
        chkNgaySinh = new CheckBox { Text = "Ngày Sinh:", Location = new Point(10, y), AutoSize = true, Checked = true };
        chkNgaySinh.CheckedChanged += (s, e) => dtpNgaySinh.Enabled = chkNgaySinh.Checked;
        dtpNgaySinh = new DateTimePicker { Location = new Point(100, y), Width = 230, Format = DateTimePickerFormat.Short };
        y += spacing;

        pnlRight.Controls.Add(new Label { Text = "Giới Tính:", Location = new Point(10, y), AutoSize = true });
        cmbGioiTinh = new ComboBox { Location = new Point(100, y), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
        cmbGioiTinh.Items.AddRange(new[] { "Nam", "Nữ" });
        cmbGioiTinh.SelectedIndex = 0;
        y += spacing;

        pnlRight.Controls.Add(new Label { Text = "SĐT (*):", Location = new Point(10, y), AutoSize = true });
        txtSDT = new TextBox { Location = new Point(100, y), Width = 230 };
        y += spacing;

        pnlRight.Controls.Add(new Label { Text = "CCCD:", Location = new Point(10, y), AutoSize = true });
        txtCCCD = new TextBox { Location = new Point(100, y), Width = 230 };
        y += spacing;

        pnlRight.Controls.Add(new Label { Text = "Địa Chỉ:", Location = new Point(10, y), AutoSize = true });
        txtDiaChi = new TextBox { Location = new Point(100, y), Width = 230, Multiline = true, Height = 60 };
        y += 80;

        btnThem = new Button { Text = "Thêm", Location = new Point(40, y), Width = 80, BackColor = Color.FromArgb(0, 85, 150), ForeColor = Color.White };
        btnSua = new Button { Text = "Cập Nhật", Location = new Point(130, y), Width = 80, BackColor = Color.FromArgb(0, 85, 150), ForeColor = Color.White };
        btnLamMoi = new Button { Text = "Làm Mới", Location = new Point(220, y), Width = 80 };
        
        btnThem.Click += BtnThem_Click;
        btnSua.Click += BtnSua_Click;
        btnLamMoi.Click += (s, e) => ResetForm();

        pnlRight.Controls.AddRange(new Control[] {
            txtHoTen, chkNgaySinh, dtpNgaySinh, cmbGioiTinh, txtSDT, txtCCCD, txtDiaChi,
            btnThem, btnSua, btnLamMoi
        });

        // Center Panel for Grid
        var pnlCenter = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        dgvBenhNhan = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            BackgroundColor = Color.White,
            RowHeadersVisible = false
        };
        dgvBenhNhan.CellClick += DgvBenhNhan_CellClick;
        pnlCenter.Controls.Add(dgvBenhNhan);

        this.Controls.Add(pnlCenter);
        this.Controls.Add(pnlRight);
        this.Controls.Add(pnlTop);
    }

    private void SetupTheme()
    {
        this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        this.BackColor = Color.FromArgb(245, 246, 250);
    }

    private void BtnSearch_Click(object? sender, EventArgs e)
    {
        LoadData(txtSearch.Text.Trim());
    }

    private void LoadData(string keyword = "")
    {
        try
        {
            var dt = _benhNhanBLL.TimBenhNhan(keyword);
            if (dt == null)
            {
                MessageBox.Show("Không thể tải danh sách bệnh nhân.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dgvBenhNhan.DataSource = null;
            }
            else
            {
                dgvBenhNhan.DataSource = dt;
                FormatGrid();
                if (dt.Rows.Count == 0 && !string.IsNullOrWhiteSpace(keyword))
                {
                    MessageBox.Show("Không tìm thấy bệnh nhân nào phù hợp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        catch
        {
            MessageBox.Show("Đã xảy ra lỗi khi tìm kiếm bệnh nhân.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void FormatGrid()
    {
        SetHeader(dgvBenhNhan, "MaBN", "Mã BN");
        SetHeader(dgvBenhNhan, "HoTen", "Họ Tên");
        SetHeader(dgvBenhNhan, "NgaySinh", "Ngày Sinh");
        SetHeader(dgvBenhNhan, "GioiTinh", "Giới Tính");
        SetHeader(dgvBenhNhan, "SDT", "SĐT");
        SetHeader(dgvBenhNhan, "CCCD", "CCCD");
        SetHeader(dgvBenhNhan, "DiaChi", "Địa Chỉ");
        SetHeader(dgvBenhNhan, "TrangThaiGanNhat", "Trạng Thái");
    }

    private void DgvBenhNhan_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && e.RowIndex < dgvBenhNhan.Rows.Count)
        {
            var row = dgvBenhNhan.Rows[e.RowIndex];
            _currentMaBN = ReadCellInt(row, "MaBN");

            txtHoTen.Text = ReadCellString(row, "HoTen");

            object? ngaySinh = ReadCellValue(row, "NgaySinh");
            if (ngaySinh is not null && ngaySinh != DBNull.Value)
            {
                chkNgaySinh.Checked = true;
                dtpNgaySinh.Value = Convert.ToDateTime(ngaySinh);
            }
            else
            {
                chkNgaySinh.Checked = false;
                dtpNgaySinh.Value = DateTime.Today;
            }

            cmbGioiTinh.SelectedItem = ReadCellString(row, "GioiTinh", "Nam");
            txtSDT.Text = ReadCellString(row, "SDT");
            txtCCCD.Text = ReadCellString(row, "CCCD");
            txtDiaChi.Text = ReadCellString(row, "DiaChi");
        }
    }

    private static object? ReadCellValue(DataGridViewRow row, string columnName)
    {
        DataGridView? grid = row.DataGridView;
        if (grid is null || !grid.Columns.Contains(columnName))
        {
            return null;
        }

        return row.Cells[columnName]?.Value;
    }

    private static string ReadCellString(DataGridViewRow row, string columnName, string fallback = "")
    {
        object? value = ReadCellValue(row, columnName);
        return value is null || value == DBNull.Value ? fallback : value.ToString() ?? fallback;
    }

    private static int ReadCellInt(DataGridViewRow row, string columnName)
    {
        object? value = ReadCellValue(row, columnName);
        return value is null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
    }

    private static void SetHeader(DataGridView grid, string columnName, string headerText)
    {
        if (grid.Columns.Contains(columnName) && grid.Columns[columnName] is DataGridViewColumn column)
        {
            column.HeaderText = headerText;
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(txtHoTen.Text))
        {
            MessageBox.Show("Họ tên không được để trống.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtHoTen.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtSDT.Text))
        {
            MessageBox.Show("Số điện thoại không được để trống.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtSDT.Focus();
            return false;
        }

        string cccd = txtCCCD.Text.Trim();
        if (!string.IsNullOrEmpty(cccd) && (cccd.Length != 12 || !cccd.All(char.IsDigit)))
        {
            MessageBox.Show("CCCD phải là 12 chữ số nếu có nhập.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtCCCD.Focus();
            return false;
        }

        return true;
    }

    private BenhNhanDTO CreateDTO()
    {
        return new BenhNhanDTO
        {
            MaBN = _currentMaBN,
            HoTen = txtHoTen.Text.Trim(),
            NgaySinh = chkNgaySinh.Checked ? dtpNgaySinh.Value : null,
            GioiTinh = cmbGioiTinh.SelectedItem?.ToString() ?? "Nam",
            SDT = txtSDT.Text.Trim(),
            CCCD = string.IsNullOrWhiteSpace(txtCCCD.Text) ? null : txtCCCD.Text.Trim(),
            DiaChi = string.IsNullOrWhiteSpace(txtDiaChi.Text) ? null : txtDiaChi.Text.Trim()
        };
    }

    private void BtnThem_Click(object? sender, EventArgs e)
    {
        if (!ValidateInput()) return;

        var dto = CreateDTO();
        dto.MaBN = 0; // Force insert
        
        bool success = _benhNhanBLL.ThemBenhNhan(dto);
        if (success)
        {
            MessageBox.Show("Thêm bệnh nhân thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ResetForm();
            LoadData();
        }
        else
        {
            MessageBox.Show("Thêm bệnh nhân thất bại. Vui lòng thử lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnSua_Click(object? sender, EventArgs e)
    {
        if (_currentMaBN <= 0)
        {
            MessageBox.Show("Vui lòng chọn bệnh nhân cần cập nhật.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!ValidateInput()) return;

        var dto = CreateDTO();
        bool success = _benhNhanBLL.CapNhatBenhNhan(dto);
        if (success)
        {
            MessageBox.Show("Cập nhật bệnh nhân thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LoadData();
        }
        else
        {
            MessageBox.Show("Cập nhật bệnh nhân thất bại. Vui lòng thử lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ResetForm()
    {
        _currentMaBN = 0;
        txtHoTen.Clear();
        chkNgaySinh.Checked = true;
        dtpNgaySinh.Value = DateTime.Now;
        cmbGioiTinh.SelectedIndex = 0;
        txtSDT.Clear();
        txtCCCD.Clear();
        txtDiaChi.Clear();
        txtHoTen.Focus();
    }
}
