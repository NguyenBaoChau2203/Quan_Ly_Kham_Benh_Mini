using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using ClinicApp.BLL;
using ClinicApp.DTO;

namespace ClinicApp.GUI.Forms;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class FrmTaoLuotKham : Form
{
    private readonly BenhNhanBLL _benhNhanBLL = new();
    private readonly LuotKhamBLL _luotKhamBLL = new();

    private TextBox txtSearch;
    private Button btnSearch;
    private DataGridView dgvBenhNhan;
    
    private TextBox txtGhiChu;
    private Button btnTaoLuot;
    private Button btnHuyLuot;
    private DataGridView dgvLuotKham;
    
    private DataTable _dtLuotKhamLocal;
    private int _selectedMaBN = 0;
    private string _selectedHoTen = "";

    public FrmTaoLuotKham()
    {
        InitializeComponent();
        SetupTheme();
        InitLocalGrid();
    }

    private void InitializeComponent()
    {
        this.Text = "Đăng ký lượt khám";
        this.Size = new Size(1000, 650);
        this.StartPosition = FormStartPosition.CenterParent;

        // Top Panel: Search and Patient Grid
        var pnlTop = new Panel { Dock = DockStyle.Top, Height = 250, Padding = new Padding(10) };
        
        var lblSearch = new Label { Text = "Tìm BN:", AutoSize = true, Location = new Point(10, 15) };
        txtSearch = new TextBox { Location = new Point(80, 12), Width = 250, PlaceholderText = "Tên, SĐT, CCCD..." };
        btnSearch = new Button { Text = "Tìm", Location = new Point(340, 10), Width = 80, BackColor = Color.FromArgb(0, 85, 150), ForeColor = Color.White };
        btnSearch.Click += BtnSearch_Click;

        dgvBenhNhan = new DataGridView
        {
            Location = new Point(10, 45),
            Width = 960,
            Height = 195,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            BackgroundColor = Color.White,
            RowHeadersVisible = false
        };
        dgvBenhNhan.CellClick += DgvBenhNhan_CellClick;

        pnlTop.Controls.AddRange(new Control[] { lblSearch, txtSearch, btnSearch, dgvBenhNhan });

        // Middle Panel: Actions
        var pnlMid = new Panel { Dock = DockStyle.Top, Height = 80, Padding = new Padding(10) };
        
        var lblGhiChu = new Label { Text = "Ghi chú:", AutoSize = true, Location = new Point(10, 20) };
        txtGhiChu = new TextBox { Location = new Point(80, 17), Width = 400 };
        
        btnTaoLuot = new Button { Text = "Tạo Lượt Khám", Location = new Point(500, 15), Width = 120, BackColor = Color.FromArgb(0, 85, 150), ForeColor = Color.White };
        btnTaoLuot.Click += BtnTaoLuot_Click;

        btnHuyLuot = new Button { Text = "Hủy Lượt Khám", Location = new Point(630, 15), Width = 120, BackColor = Color.IndianRed, ForeColor = Color.White };
        btnHuyLuot.Click += BtnHuyLuot_Click;

        pnlMid.Controls.AddRange(new Control[] { lblGhiChu, txtGhiChu, btnTaoLuot, btnHuyLuot });

        // Bottom Panel: Local queue
        var pnlBottom = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
        var lblList = new Label { Text = "Các lượt khám vừa đăng ký:", AutoSize = true, Location = new Point(10, 0) };
        
        dgvLuotKham = new DataGridView
        {
            Location = new Point(10, 25),
            Width = 960,
            Height = 250,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            BackgroundColor = Color.White,
            RowHeadersVisible = false
        };
        
        pnlBottom.Controls.AddRange(new Control[] { lblList, dgvLuotKham });

        dgvLuotKham.CellFormatting += DgvLuotKham_CellFormatting;

        this.Controls.Add(pnlBottom);
        this.Controls.Add(pnlMid);
        this.Controls.Add(pnlTop);
    }

    private void SetupTheme()
    {
        this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        this.BackColor = Color.FromArgb(245, 246, 250);
    }

    private void InitLocalGrid()
    {
        _dtLuotKhamLocal = new DataTable();
        _dtLuotKhamLocal.Columns.Add("MaLK", typeof(int));
        _dtLuotKhamLocal.Columns.Add("SoThuTu", typeof(int));
        _dtLuotKhamLocal.Columns.Add("MaBN", typeof(int));
        _dtLuotKhamLocal.Columns.Add("HoTen", typeof(string));
        _dtLuotKhamLocal.Columns.Add("NgayKham", typeof(DateTime));
        _dtLuotKhamLocal.Columns.Add("TrangThai", typeof(string));

        dgvLuotKham.DataSource = _dtLuotKhamLocal;
        FormatGridLuotKham();
    }

    private void FormatGridLuotKham()
    {
        SetHeader(dgvLuotKham, "MaLK", "Mã LK");
        SetHeader(dgvLuotKham, "SoThuTu", "STT");
        SetHeader(dgvLuotKham, "MaBN", "Mã BN");
        SetHeader(dgvLuotKham, "HoTen", "Họ Tên");
        SetHeader(dgvLuotKham, "NgayKham", "Ngày Khám");
        SetHeader(dgvLuotKham, "TrangThai", "Trạng Thái");
    }

    private void DgvLuotKham_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (dgvLuotKham.Columns[e.ColumnIndex].Name == "TrangThai" && e.Value is string status)
        {
            e.Value = status switch
            {
                "DangCho" => "Đang chờ",
                "DangKham" => "Đang khám",
                "DaKham" => "Đã khám",
                "DaHuy" => "Đã hủy",
                _ => status
            };
            e.FormattingApplied = true;
        }
    }

    private void BtnSearch_Click(object? sender, EventArgs e)
    {
        try
        {
            var dt = _benhNhanBLL.TimBenhNhan(txtSearch.Text.Trim());
            if (dt == null)
            {
                MessageBox.Show("Không thể tải danh sách bệnh nhân.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dgvBenhNhan.DataSource = null;
            }
            else
            {
                dgvBenhNhan.DataSource = dt;
                FormatGridBenhNhan();
                if (dt.Rows.Count == 0 && !string.IsNullOrWhiteSpace(txtSearch.Text.Trim()))
                {
                    MessageBox.Show("Không tìm thấy bệnh nhân nào phù hợp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        catch
        {
            MessageBox.Show("Đã xảy ra lỗi khi tìm kiếm.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void FormatGridBenhNhan()
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
            _selectedMaBN = ReadCellInt(row, "MaBN");
            _selectedHoTen = ReadCellString(row, "HoTen");
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

    private void BtnTaoLuot_Click(object? sender, EventArgs e)
    {
        if (_selectedMaBN <= 0)
        {
            MessageBox.Show("Vui lòng chọn một bệnh nhân để tạo lượt khám.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            // Null cho maBacSi vi day la dang ky tu tiep nhan
            var lk = _luotKhamBLL.TaoLuotKham(_selectedMaBN, null, txtGhiChu.Text.Trim());
            
            if (lk != null)
            {
                MessageBox.Show($"Tạo lượt khám thành công!\nSố thứ tự: {lk.SoThuTu}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Add to local grid
                _dtLuotKhamLocal.Rows.Add(lk.MaLK, lk.SoThuTu, lk.MaBN, _selectedHoTen, lk.NgayKham, lk.TrangThai);
                txtGhiChu.Clear();
            }
            else
            {
                MessageBox.Show("Không thể tạo lượt khám. Vui lòng thử lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch
        {
            MessageBox.Show("Lỗi khi tạo lượt khám. Có thể CSDL chưa sẵn sàng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnHuyLuot_Click(object? sender, EventArgs e)
    {
        if (dgvLuotKham.SelectedRows.Count == 0)
        {
            MessageBox.Show("Vui lòng chọn lượt khám cần hủy.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var row = dgvLuotKham.SelectedRows[0];
        int maLK = ReadCellInt(row, "MaLK");
        string trangThai = ReadCellString(row, "TrangThai");

        if (maLK <= 0)
        {
            MessageBox.Show("Không xác định được lượt khám cần hủy.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (trangThai != "DangCho")
        {
            MessageBox.Show("Chỉ có thể hủy lượt khám đang ở trạng thái chờ.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show("Bạn có chắc chắn muốn hủy lượt khám này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            try
            {
                bool success = _luotKhamBLL.HuyLuotKham(maLK);
                if (success)
                {
                    MessageBox.Show("Đã hủy lượt khám.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    row.Cells["TrangThai"].Value = "DaHuy";
                }
                else
                {
                    MessageBox.Show("Hủy lượt khám thất bại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch
            {
                MessageBox.Show("Lỗi khi hủy lượt khám.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
