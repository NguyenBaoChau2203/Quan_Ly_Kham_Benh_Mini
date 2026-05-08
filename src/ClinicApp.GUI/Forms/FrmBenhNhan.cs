using System.Data;
using ClinicApp.BLL;
using ClinicApp.DTO;

namespace ClinicApp.GUI.Forms;

public class FrmBenhNhan : Form
{
    private readonly BenhNhanBLL _benhNhanBLL = new();
    private readonly TextBox _txtSearchName = NativeUi.TextBox("Nhập tên bệnh nhân...");
    private readonly TextBox _txtSearchPhone = NativeUi.TextBox("09xx...");
    private readonly TextBox _txtSearchId = NativeUi.TextBox("Nhập số CCCD...");
    private readonly DataGridView _grid = NativeUi.Grid();
    private readonly TextBox _txtHoTen = NativeUi.TextBox();
    private readonly DateTimePicker _dtpNgaySinh = new();
    private readonly CheckBox _chkNgaySinh = new() { Text = "Có ngày sinh", Checked = true, Height = 22, Dock = DockStyle.Top };
    private readonly ComboBox _cmbGioiTinh = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _txtSDT = NativeUi.TextBox();
    private readonly TextBox _txtCCCD = NativeUi.TextBox();
    private readonly TextBox _txtDiaChi = NativeUi.MultilineTextBox(height: 78);
    private readonly Label _lblFooter = new() { Dock = DockStyle.Bottom, Height = 28, ForeColor = UiTheme.MutedText, Font = UiTheme.SmallFont };

    private int _currentMaBN;

    public FrmBenhNhan()
    {
        UiTheme.ApplyForm(this);
        Text = "Quản lý bệnh nhân";
        BuildLayout();
        Load += (_, _) => { ResetForm(); LoadPatients(); };
    }

    private void BuildLayout()
    {
        var page = NativeUi.Page();
        Controls.Add(page);

        var searchCard = NativeUi.Card(DockStyle.Top, height: 104);
        page.Controls.Add(searchCard);

        var searchGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1,
            Padding = new Padding(8, 0, 8, 0)
        };
        searchGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
        searchGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
        searchGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 24));
        searchGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 144));
        searchGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 206));

        foreach (TextBox box in new[] { _txtSearchName, _txtSearchPhone, _txtSearchId })
        {
            box.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) LoadPatients(); };
        }

        var btnSearch = NativeUi.PrimaryButton("TÌM KIẾM");
        btnSearch.Dock = DockStyle.Fill;
        btnSearch.Margin = new Padding(8, 20, 4, 20);
        btnSearch.Click += (_, _) => LoadPatients();

        var btnNew = NativeUi.SecondaryButton("+  THÊM BỆNH NHÂN MỚI");
        btnNew.Dock = DockStyle.Fill;
        btnNew.Margin = new Padding(4, 20, 0, 20);
        btnNew.Click += (_, _) => ResetForm();

        searchGrid.Controls.Add(NativeUi.Field("Họ và tên", _txtSearchName), 0, 0);
        searchGrid.Controls.Add(NativeUi.Field("Số điện thoại", _txtSearchPhone), 1, 0);
        searchGrid.Controls.Add(NativeUi.Field("CCCD / Định danh", _txtSearchId), 2, 0);
        searchGrid.Controls.Add(btnSearch, 3, 0);
        searchGrid.Controls.Add(btnNew, 4, 0);
        searchCard.Controls.Add(searchGrid);

        var body = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel2,
            BackColor = UiTheme.Background
        };
        NativeUi.ConfigureSplitter(body, desiredDistance: 760, panel1MinSize: 420, panel2MinSize: 320);
        page.Controls.Add(body);
        body.BringToFront();

        var listCard = NativeUi.Card(DockStyle.Fill);
        body.Panel1.Controls.Add(listCard);
        listCard.Controls.Add(_grid);
        listCard.Controls.Add(NativeUi.Section("Danh sách bệnh nhân tiếp nhận"));
        listCard.Controls.Add(_lblFooter);
        _grid.SelectionChanged += (_, _) => FillSelectedPatient();

        var detailCard = NativeUi.Card(DockStyle.Fill, width: 370);
        detailCard.Padding = new Padding(14);
        body.Panel2.Controls.Add(detailCard);

        _cmbGioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
        UiTheme.ApplyComboBox(_cmbGioiTinh);
        UiTheme.ApplyDateTimePicker(_dtpNgaySinh);
        _chkNgaySinh.CheckedChanged += (_, _) => _dtpNgaySinh.Enabled = _chkNgaySinh.Checked;

        var btnSave = NativeUi.PrimaryButton("LƯU THAY ĐỔI");
        btnSave.Dock = DockStyle.Top;
        btnSave.Click += (_, _) => SavePatient();

        var btnCancel = NativeUi.SecondaryButton("HỦY BỎ");
        btnCancel.Dock = DockStyle.Top;
        btnCancel.Margin = new Padding(0, 8, 0, 0);
        btnCancel.Click += (_, _) => ResetForm();

        detailCard.Controls.Add(btnCancel);
        detailCard.Controls.Add(btnSave);
        detailCard.Controls.Add(NativeUi.Field("Địa chỉ thường trú", _txtDiaChi));
        detailCard.Controls.Add(NativeUi.Field("Số CCCD", _txtCCCD));
        detailCard.Controls.Add(NativeUi.Field("Số điện thoại *", _txtSDT));
        detailCard.Controls.Add(NativeUi.Field("Giới tính", _cmbGioiTinh));
        detailCard.Controls.Add(NativeUi.Field("Ngày sinh", _dtpNgaySinh));
        detailCard.Controls.Add(_chkNgaySinh);
        detailCard.Controls.Add(NativeUi.Field("Họ tên bệnh nhân *", _txtHoTen));
        detailCard.Controls.Add(NativeUi.Section("Thông tin chi tiết"));
    }

    private void LoadPatients()
    {
        try
        {
            DataTable table = _benhNhanBLL.TimBenhNhan(BuildKeyword());
            _grid.DataSource = table;
            ConfigureGrid();
            _lblFooter.Text = $"Tổng cộng: {table.Rows.Count} bệnh nhân";
        }
        catch (Exception ex)
        {
            NativeUi.ShowError("Không tải được danh sách bệnh nhân.\n" + ex.Message);
        }
    }

    private string BuildKeyword()
    {
        if (!string.IsNullOrWhiteSpace(_txtSearchName.Text)) return _txtSearchName.Text.Trim();
        if (!string.IsNullOrWhiteSpace(_txtSearchPhone.Text)) return _txtSearchPhone.Text.Trim();
        if (!string.IsNullOrWhiteSpace(_txtSearchId.Text)) return _txtSearchId.Text.Trim();
        return string.Empty;
    }

    private void ConfigureGrid()
    {
        if (_grid.Columns.Count == 0) return;
        SetHeader("MaBN", "Mã BN", 70);
        SetHeader("HoTen", "Họ tên", 180);
        SetHeader("NgaySinh", "Ngày sinh", 90);
        SetHeader("GioiTinh", "Giới tính", 70);
        SetHeader("SDT", "Điện thoại", 110);
        SetHeader("CCCD", "CCCD", 120);
        SetHeader("DiaChi", "Địa chỉ", 220);
    }

    private void SetHeader(string columnName, string header, int width)
    {
        if (!_grid.Columns.Contains(columnName)) return;
        _grid.Columns[columnName].HeaderText = header;
        _grid.Columns[columnName].Width = width;
    }

    private void FillSelectedPatient()
    {
        if (_grid.CurrentRow?.DataBoundItem is not DataRowView view) return;
        DataRow row = view.Row;

        _currentMaBN = NativeUi.IntOf(row, "MaBN");
        _txtHoTen.Text = NativeUi.TextOf(row, "HoTen");
        _txtSDT.Text = NativeUi.TextOf(row, "SDT");
        _txtCCCD.Text = NativeUi.TextOf(row, "CCCD");
        _txtDiaChi.Text = NativeUi.TextOf(row, "DiaChi");
        _cmbGioiTinh.Text = string.IsNullOrWhiteSpace(NativeUi.TextOf(row, "GioiTinh")) ? "Nam" : NativeUi.TextOf(row, "GioiTinh");

        DateTime? ngaySinh = NativeUi.DateOf(row, "NgaySinh");
        _chkNgaySinh.Checked = ngaySinh.HasValue;
        _dtpNgaySinh.Enabled = ngaySinh.HasValue;
        _dtpNgaySinh.Value = ngaySinh ?? DateTime.Today;
    }

    private void ResetForm()
    {
        _currentMaBN = 0;
        _txtHoTen.Clear();
        _txtSDT.Clear();
        _txtCCCD.Clear();
        _txtDiaChi.Clear();
        _cmbGioiTinh.SelectedIndex = 0;
        _chkNgaySinh.Checked = true;
        _dtpNgaySinh.Value = DateTime.Today;
        _txtHoTen.Focus();
    }

    private void SavePatient()
    {
        string hoten = _txtHoTen.Text.Trim();
        string sdt = _txtSDT.Text.Trim();
        string cccd = _txtCCCD.Text.Trim();

        if (hoten.Length == 0)
        {
            NativeUi.ShowError("Họ tên không được để trống.");
            _txtHoTen.Focus();
            return;
        }

        if (sdt.Length == 0)
        {
            NativeUi.ShowError("SĐT không được để trống.");
            _txtSDT.Focus();
            return;
        }

        if (!sdt.StartsWith('0') || sdt.Length is < 10 or > 11 || !sdt.All(char.IsDigit))
        {
            NativeUi.ShowError("SĐT phải bắt đầu bằng 0 và dài 10-11 số.");
            _txtSDT.Focus();
            return;
        }

        if (cccd.Length > 0 && (cccd.Length != 12 || !cccd.All(char.IsDigit)))
        {
            NativeUi.ShowError("CCCD phải đúng 12 số.");
            _txtCCCD.Focus();
            return;
        }

        var dto = new BenhNhanDTO
        {
            MaBN = _currentMaBN,
            HoTen = hoten,
            NgaySinh = _chkNgaySinh.Checked ? _dtpNgaySinh.Value.Date : null,
            GioiTinh = _cmbGioiTinh.Text,
            SDT = sdt,
            CCCD = string.IsNullOrWhiteSpace(cccd) ? null : cccd,
            DiaChi = string.IsNullOrWhiteSpace(_txtDiaChi.Text) ? null : _txtDiaChi.Text.Trim()
        };

        bool ok = _currentMaBN > 0
            ? _benhNhanBLL.CapNhatBenhNhan(dto)
            : _benhNhanBLL.ThemBenhNhan(dto);

        if (!ok)
        {
            NativeUi.ShowError("Thao tác thất bại. Có thể SĐT/CCCD bị trùng hoặc dữ liệu chưa hợp lệ.");
            return;
        }

        NativeUi.ShowInfo("Lưu bệnh nhân thành công.");
        ResetForm();
        LoadPatients();
    }
}
