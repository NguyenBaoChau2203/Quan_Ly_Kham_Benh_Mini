using System.Data;
using ClinicApp.BLL;

namespace ClinicApp.GUI.Forms;

public class FrmLichSu : Form
{
    private readonly ThongKeBLL _thongKeBLL = new();
    private readonly DateTimePicker _dtpFrom = new();
    private readonly DateTimePicker _dtpTo = new();
    private readonly TextBox _txtKeyword = NativeUi.TextBox("Mã BN, họ tên, SĐT hoặc CCCD...");
    private readonly DataGridView _grid = NativeUi.Grid();
    private readonly Label _lblSummary = new()
    {
        Dock = DockStyle.Fill,
        Font = UiTheme.LabelFont,
        ForeColor = UiTheme.MutedText,
        TextAlign = ContentAlignment.MiddleLeft
    };

    public FrmLichSu()
    {
        UiTheme.ApplyForm(this);
        Text = "Lịch sử khám bệnh";
        BuildLayout();
        Load += (_, _) =>
        {
            _dtpTo.Value = DateTime.Today;
            _dtpFrom.Value = DateTime.Today.AddDays(-7);
            LoadData();
        };
    }

    private void BuildLayout()
    {
        var page = NativeUi.Page();
        Controls.Add(page);

        var filter = NativeUi.Card(DockStyle.Top, 92);
        page.Controls.Add(filter);
        filter.Controls.Add(NativeUi.Title("Lịch sử khám bệnh"));

        UiTheme.ApplyDateTimePicker(_dtpFrom);
        UiTheme.ApplyDateTimePicker(_dtpTo);
        _dtpFrom.Format = DateTimePickerFormat.Custom;
        _dtpFrom.CustomFormat = "dd/MM/yyyy";
        _dtpTo.Format = DateTimePickerFormat.Custom;
        _dtpTo.CustomFormat = "dd/MM/yyyy";
        _dtpFrom.Width = 118;
        _dtpTo.Width = 118;
        _txtKeyword.Width = 300;
        _txtKeyword.Dock = DockStyle.Fill;
        _txtKeyword.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) LoadData(); };

        var toolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 38,
            ColumnCount = 7,
            BackColor = UiTheme.SurfaceContainerLowest
        };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 126));
        toolbar.Controls.Add(NativeUi.FieldLabel("Từ ngày"), 0, 0);
        toolbar.Controls.Add(_dtpFrom, 1, 0);
        toolbar.Controls.Add(NativeUi.FieldLabel("Đến ngày"), 2, 0);
        toolbar.Controls.Add(_dtpTo, 3, 0);
        toolbar.Controls.Add(NativeUi.FieldLabel("Tìm kiếm"), 4, 0);
        toolbar.Controls.Add(_txtKeyword, 5, 0);

        var btnFilter = NativeUi.PrimaryButton("Lọc dữ liệu");
        btnFilter.Dock = DockStyle.Fill;
        btnFilter.Click += (_, _) => LoadData();
        toolbar.Controls.Add(btnFilter, 6, 0);
        filter.Controls.Add(toolbar);

        var gridCard = NativeUi.Card(DockStyle.Fill);
        page.Controls.Add(gridCard);
        gridCard.BringToFront();
        gridCard.Controls.Add(_grid);

        var bottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 34,
            BackColor = UiTheme.SurfaceContainerLow,
            Padding = new Padding(12, 0, 12, 0)
        };
        page.Controls.Add(bottom);
        bottom.Controls.Add(_lblSummary);
    }

    private void LoadData()
    {
        try
        {
            DataTable table = _thongKeBLL.LayLichSuKham(_dtpFrom.Value.Date, _dtpTo.Value.Date, _txtKeyword.Text);
            _grid.DataSource = table;
            ConfigureGrid();

            int daKham = table.AsEnumerable().Count(r => NativeUi.TextOf(r, "TrangThai") == "DaKham");
            int dangKham = table.AsEnumerable().Count(r => NativeUi.TextOf(r, "TrangThai") == "DangKham");
            int daHuy = table.AsEnumerable().Count(r => NativeUi.TextOf(r, "TrangThai") == "DaHuy");
            _lblSummary.Text = $"Tổng cộng: {table.Rows.Count} bản ghi | Đã khám: {daKham} | Đang khám: {dangKham} | Đã hủy: {daHuy}";
        }
        catch (Exception ex)
        {
            NativeUi.ShowError("Đã xảy ra lỗi khi tải lịch sử khám.\n" + ex.Message);
        }
    }

    private void ConfigureGrid()
    {
        Header("MaLK", "Mã LK", 70);
        Header("NgayKham", "Ngày khám", 145);
        Header("MaBN", "Mã BN", 70);
        Header("HoTen", "Họ tên", 190);
        Header("TenBacSi", "Bác sĩ", 160);
        Header("ChanDoan", "Chẩn đoán", 260);
        Header("TrangThai", "Trạng thái", 110);
    }

    private void Header(string name, string text, int width)
    {
        if (!_grid.Columns.Contains(name)) return;
        _grid.Columns[name].HeaderText = text;
        _grid.Columns[name].Width = width;
    }
}
