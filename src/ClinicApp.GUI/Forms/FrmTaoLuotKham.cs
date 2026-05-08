using System.Data;
using ClinicApp.BLL;
using ClinicApp.DTO;

namespace ClinicApp.GUI.Forms;

public class FrmTaoLuotKham : Form
{
    private readonly BenhNhanBLL _benhNhanBLL = new();
    private readonly LuotKhamBLL _luotKhamBLL = new();
    private readonly TextBox _txtSearch = NativeUi.TextBox("Nhập tên bệnh nhân, SĐT hoặc CCCD...");
    private readonly DataGridView _gridPatients = NativeUi.Grid();
    private readonly DataGridView _gridQueue = NativeUi.Grid();
    private readonly TextBox _txtGhiChu = NativeUi.MultilineTextBox("Nhập triệu chứng hoặc lý do khám...", 92);
    private readonly Label _lblPatient = new()
    {
        Dock = DockStyle.Top,
        Height = 76,
        Font = UiTheme.SectionHeaderFont,
        ForeColor = UiTheme.Primary,
        Text = "Chưa chọn bệnh nhân",
        TextAlign = ContentAlignment.MiddleLeft
    };

    private readonly BindingSource _localQueue = new();
    private BenhNhanDTO? _selectedPatient;

    public FrmTaoLuotKham()
    {
        UiTheme.ApplyForm(this);
        Text = "Đăng ký lượt khám";
        BuildLayout();
    }

    private void BuildLayout()
    {
        var page = NativeUi.Page();
        Controls.Add(page);

        var searchCard = NativeUi.Card(DockStyle.Top, 74);
        page.Controls.Add(searchCard);

        var toolbar = NativeUi.Toolbar();
        _txtSearch.Width = 440;
        _txtSearch.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) SearchPatients(); };
        var btnSearch = NativeUi.PrimaryButton("Tìm kiếm");
        btnSearch.Width = 120;
        btnSearch.Click += (_, _) => SearchPatients();
        toolbar.Controls.Add(NativeUi.FieldLabel("Tìm bệnh nhân"));
        toolbar.Controls.Add(_txtSearch);
        toolbar.Controls.Add(btnSearch);
        searchCard.Controls.Add(toolbar);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            BackColor = UiTheme.Background
        };
        NativeUi.ConfigureSplitter(split, desiredDistance: 330, panel1MinSize: 220, panel2MinSize: 180);
        page.Controls.Add(split);
        split.BringToFront();

        var topSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel2
        };
        NativeUi.ConfigureSplitter(topSplit, desiredDistance: 650, panel1MinSize: 360, panel2MinSize: 300);
        split.Panel1.Controls.Add(topSplit);

        var resultCard = NativeUi.Card(DockStyle.Fill);
        topSplit.Panel1.Controls.Add(resultCard);
        resultCard.Controls.Add(_gridPatients);
        resultCard.Controls.Add(NativeUi.Section("Kết quả tìm kiếm"));
        _gridPatients.SelectionChanged += (_, _) => SelectPatientFromGrid();

        var actionCard = NativeUi.Card(DockStyle.Fill);
        topSplit.Panel2.Controls.Add(actionCard);
        actionCard.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 26,
            Text = "Thông tin lượt khám",
            Font = UiTheme.SectionHeaderFont,
            ForeColor = UiTheme.Primary
        });
        actionCard.Controls.Add(_lblPatient);
        actionCard.Controls.Add(NativeUi.Field("Ghi chú / lý do khám", _txtGhiChu));

        var btnCreate = NativeUi.PrimaryButton("Đăng ký khám");
        btnCreate.Dock = DockStyle.Top;
        btnCreate.Click += (_, _) => CreateVisit();
        actionCard.Controls.Add(btnCreate);

        var btnCancel = NativeUi.DangerButton("Hủy lượt đã chọn");
        btnCancel.Dock = DockStyle.Top;
        btnCancel.Margin = new Padding(0, 8, 0, 0);
        btnCancel.Click += (_, _) => CancelSelectedVisit();
        actionCard.Controls.Add(btnCancel);

        var queueCard = NativeUi.Card(DockStyle.Fill);
        split.Panel2.Controls.Add(queueCard);
        queueCard.Controls.Add(_gridQueue);
        queueCard.Controls.Add(NativeUi.Section("Các lượt khám vừa đăng ký"));
        _gridQueue.DataSource = _localQueue;
    }

    private void SearchPatients()
    {
        try
        {
            DataTable table = _benhNhanBLL.TimBenhNhan(_txtSearch.Text);
            _gridPatients.DataSource = table;
            ConfigurePatientGrid();
            if (table.Rows.Count == 0)
            {
                NativeUi.ShowInfo("Không tìm thấy bệnh nhân nào.");
            }
        }
        catch (Exception ex)
        {
            NativeUi.ShowError("Không tìm kiếm được bệnh nhân.\n" + ex.Message);
        }
    }

    private void ConfigurePatientGrid()
    {
        if (_gridPatients.Columns.Count == 0) return;
        Header(_gridPatients, "MaBN", "Mã BN", 70);
        Header(_gridPatients, "HoTen", "Họ tên", 180);
        Header(_gridPatients, "SDT", "SĐT", 110);
        Header(_gridPatients, "NgaySinh", "Ngày sinh", 90);
        Header(_gridPatients, "GioiTinh", "Giới tính", 80);
        Header(_gridPatients, "DiaChi", "Địa chỉ", 220);
    }

    private static void Header(DataGridView grid, string name, string text, int width)
    {
        if (!grid.Columns.Contains(name)) return;
        grid.Columns[name].HeaderText = text;
        grid.Columns[name].Width = width;
    }

    private void SelectPatientFromGrid()
    {
        if (_gridPatients.CurrentRow?.DataBoundItem is not DataRowView view) return;
        DataRow row = view.Row;
        _selectedPatient = new BenhNhanDTO
        {
            MaBN = NativeUi.IntOf(row, "MaBN"),
            HoTen = NativeUi.TextOf(row, "HoTen"),
            NgaySinh = NativeUi.DateOf(row, "NgaySinh"),
            GioiTinh = NativeUi.TextOf(row, "GioiTinh"),
            SDT = NativeUi.TextOf(row, "SDT"),
            CCCD = NativeUi.TextOf(row, "CCCD"),
            DiaChi = NativeUi.TextOf(row, "DiaChi")
        };

        _lblPatient.Text =
            $"{_selectedPatient.HoTen}\nMã BN: {_selectedPatient.MaBN} | SĐT: {_selectedPatient.SDT} | " +
            $"Ngày sinh: {(_selectedPatient.NgaySinh.HasValue ? _selectedPatient.NgaySinh.Value.ToString("dd/MM/yyyy") : "--")}";
    }

    private void CreateVisit()
    {
        if (_selectedPatient == null)
        {
            NativeUi.ShowError("Vui lòng chọn một bệnh nhân.");
            return;
        }

        try
        {
            LuotKhamDTO? visit = _luotKhamBLL.TaoLuotKham(_selectedPatient.MaBN, null, _txtGhiChu.Text);
            if (visit == null)
            {
                NativeUi.ShowError("Không thể tạo lượt khám.");
                return;
            }

            var rows = _localQueue.DataSource as List<LocalVisit> ?? new List<LocalVisit>();
            rows.Insert(0, new LocalVisit
            {
                MaLK = visit.MaLK,
                SoThuTu = visit.SoThuTu,
                MaBN = visit.MaBN,
                HoTen = _selectedPatient.HoTen,
                NgayKham = visit.NgayKham,
                TrangThai = NativeUi.StatusText(visit.TrangThai)
            });

            _localQueue.DataSource = rows;
            _localQueue.ResetBindings(false);
            ConfigureQueueGrid();
            _txtGhiChu.Clear();
            NativeUi.ShowInfo($"Tạo lượt khám thành công. Số thứ tự: {visit.SoThuTu}");
        }
        catch (Exception ex)
        {
            NativeUi.ShowError("Không thể tạo lượt khám.\n" + ex.Message);
        }
    }

    private void CancelSelectedVisit()
    {
        if (_gridQueue.CurrentRow?.DataBoundItem is not LocalVisit visit)
        {
            NativeUi.ShowError("Chọn lượt khám cần hủy từ bảng bên dưới.");
            return;
        }

        if (visit.TrangThai != "Đang chờ")
        {
            NativeUi.ShowError("Chỉ hủy được lượt khám đang chờ.");
            return;
        }

        if (MessageBox.Show("Bạn có chắc chắn muốn hủy lượt khám này?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        if (!_luotKhamBLL.HuyLuotKham(visit.MaLK))
        {
            NativeUi.ShowError("Hủy lượt khám thất bại.");
            return;
        }

        visit.TrangThai = "Đã hủy";
        _localQueue.ResetBindings(false);
        NativeUi.ShowInfo("Đã hủy lượt khám thành công.");
    }

    private void ConfigureQueueGrid()
    {
        Header(_gridQueue, nameof(LocalVisit.MaLK), "Mã LK", 70);
        Header(_gridQueue, nameof(LocalVisit.SoThuTu), "STT", 60);
        Header(_gridQueue, nameof(LocalVisit.MaBN), "Mã BN", 70);
        Header(_gridQueue, nameof(LocalVisit.HoTen), "Họ tên", 180);
        Header(_gridQueue, nameof(LocalVisit.NgayKham), "Ngày khám", 150);
        Header(_gridQueue, nameof(LocalVisit.TrangThai), "Trạng thái", 100);
    }

    private sealed class LocalVisit
    {
        public int MaLK { get; set; }
        public int SoThuTu { get; set; }
        public int MaBN { get; set; }
        public string HoTen { get; set; } = "";
        public DateTime NgayKham { get; set; }
        public string TrangThai { get; set; } = "";
    }
}
