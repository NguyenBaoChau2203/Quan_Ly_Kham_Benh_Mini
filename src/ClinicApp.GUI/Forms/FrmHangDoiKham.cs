using System.Data;
using ClinicApp.BLL;
using ClinicApp.DTO;

namespace ClinicApp.GUI.Forms;

public class FrmHangDoiKham : Form
{
    private readonly KhamBLL _khamBLL = new();
    private readonly NhanVienDTO? _currentUser;
    private readonly DataGridView _grid = NativeUi.Grid();
    private readonly Label _emptyLabel = new()
    {
        Dock = DockStyle.Fill,
        Font = UiTheme.SectionHeaderFont,
        ForeColor = UiTheme.MutedText,
        Text = "Chưa có lượt khám đang chờ.\n\nNhân viên tiếp nhận cần tạo lượt khám trước, sau đó danh sách sẽ xuất hiện ở đây.",
        TextAlign = ContentAlignment.MiddleCenter,
        Visible = false
    };
    private readonly Label _lblStatus = new()
    {
        Dock = DockStyle.Left,
        Width = 360,
        Font = UiTheme.LabelFont,
        ForeColor = UiTheme.MutedText,
        TextAlign = ContentAlignment.MiddleLeft
    };
    private readonly Button _btnStart = NativeUi.PrimaryButton("Bắt đầu khám");
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 15000 };

    public event EventHandler<ExamStartedEventArgs>? ExamStarted;

    public FrmHangDoiKham(NhanVienDTO? currentUser)
    {
        _currentUser = currentUser;
        UiTheme.ApplyForm(this);
        Text = "Danh sách hàng đợi khám";
        BuildLayout();
        Load += (_, _) => RefreshQueue();
        _timer.Tick += (_, _) => RefreshQueue(quiet: true);
        _timer.Start();
        FormClosed += (_, _) => _timer.Stop();
    }

    private void BuildLayout()
    {
        var page = NativeUi.Page();
        Controls.Add(page);

        var header = NativeUi.Card(DockStyle.Top, 68);
        page.Controls.Add(header);

        var title = NativeUi.Title("Danh sách hàng đợi khám");
        header.Controls.Add(title);

        var toolbar = NativeUi.Toolbar();
        var btnRefresh = NativeUi.SecondaryButton("Làm mới");
        btnRefresh.Width = 110;
        btnRefresh.Click += (_, _) => RefreshQueue();
        _btnStart.Width = 150;
        _btnStart.Enabled = false;
        _btnStart.Click += (_, _) => StartExam();
        toolbar.Controls.Add(btnRefresh);
        toolbar.Controls.Add(_btnStart);
        toolbar.Controls.Add(_lblStatus);
        header.Controls.Add(toolbar);

        var gridCard = NativeUi.Card(DockStyle.Fill);
        page.Controls.Add(gridCard);
        gridCard.BringToFront();
        gridCard.Controls.Add(_grid);
        gridCard.Controls.Add(_emptyLabel);
        _emptyLabel.BringToFront();
        _grid.SelectionChanged += (_, _) => UpdateSelectionState();

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 34,
            BackColor = UiTheme.SurfaceContainerLowest,
            Padding = new Padding(12, 0, 12, 0)
        };
        page.Controls.Add(footer);
        footer.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = "Chọn một lượt khám đang chờ rồi bấm Bắt đầu khám.",
            Font = UiTheme.SmallFont,
            ForeColor = UiTheme.MutedText,
            TextAlign = ContentAlignment.MiddleLeft
        });
    }

    private void RefreshQueue(bool quiet = false)
    {
        try
        {
            DataTable table = _khamBLL.LayHangDoiDangCho();
            _grid.DataSource = table;
            ConfigureGrid();
            _lblStatus.Text = table.Rows.Count == 0
                ? "Chưa có lượt khám đang chờ"
                : $"Có {table.Rows.Count} lượt khám đang chờ";
            _emptyLabel.Visible = table.Rows.Count == 0;
            _grid.Visible = table.Rows.Count > 0;
            UpdateSelectionState();
        }
        catch (Exception ex)
        {
            if (!quiet)
            {
                NativeUi.ShowError("Không tải được hàng đợi khám.\n" + ex.Message);
            }
        }
    }

    private void ConfigureGrid()
    {
        Header("SoThuTu", "STT", 60);
        Header("MaLK", "Mã LK", 70);
        Header("MaBN", "Mã BN", 70);
        Header("HoTen", "Họ tên", 220);
        Header("NgayKham", "Ngày khám", 150);
        Header("ThoiGianChoPhut", "Chờ (phút)", 90);
        Header("TrangThai", "Trạng thái", 100);
    }

    private void Header(string name, string text, int width)
    {
        if (!_grid.Columns.Contains(name)) return;
        _grid.Columns[name].HeaderText = text;
        _grid.Columns[name].Width = width;
    }

    private void UpdateSelectionState()
    {
        _btnStart.Enabled = _grid.CurrentRow?.DataBoundItem is DataRowView;
    }

    private void StartExam()
    {
        if (_grid.CurrentRow?.DataBoundItem is not DataRowView view) return;
        int maLK = NativeUi.IntOf(view.Row, "MaLK");
        if (maLK <= 0) return;

        int doctorId = _currentUser?.MaNV ?? 0;
        if (doctorId <= 0)
        {
            NativeUi.ShowError("Không xác định được bác sĩ đang đăng nhập.");
            return;
        }

        if (MessageBox.Show("Chuyển lượt khám đã chọn sang trạng thái Đang khám?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            if (_khamBLL.ChuyenSangDangKham(maLK, doctorId))
            {
                ExamStarted?.Invoke(this, new ExamStartedEventArgs(maLK));
                RefreshQueue(quiet: true);
            }
            else
            {
                NativeUi.ShowError("Lượt khám này không còn ở trạng thái đang chờ.");
                RefreshQueue();
            }
        }
        catch (Exception ex)
        {
            NativeUi.ShowError("Lỗi hệ thống khi bắt đầu khám.\n" + ex.Message);
            RefreshQueue(quiet: true);
        }
    }
}

public sealed class ExamStartedEventArgs : EventArgs
{
    public ExamStartedEventArgs(int maLK)
    {
        MaLK = maLK;
    }

    public int MaLK { get; }
}
