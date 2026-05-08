using System.Data;
using ClinicApp.BLL;

namespace ClinicApp.GUI.Forms;

public class FrmDashboard : Form
{
    private readonly ThongKeBLL _thongKeBLL = new();
    private readonly KhamBLL _khamBLL = new();
    private readonly Label _lblTotalToday = MetricLabel();
    private readonly Label _lblCompletedToday = MetricLabel();
    private readonly Label _lblWaitingToday = MetricLabel();
    private readonly Label _lblTotal7Days = MetricLabel();
    private readonly DataGridView _gridStats = NativeUi.Grid();
    private readonly DataGridView _gridQueue = NativeUi.Grid();

    public FrmDashboard()
    {
        UiTheme.ApplyForm(this);
        Text = "Bảng điều khiển";
        BuildLayout();
        Load += (_, _) => LoadData();
    }

    private static Label MetricLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 24F, FontStyle.Bold),
            ForeColor = UiTheme.Primary,
            TextAlign = ContentAlignment.MiddleLeft,
            Text = "0"
        };
    }

    private void BuildLayout()
    {
        var page = NativeUi.Page();
        Controls.Add(page);

        var header = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = UiTheme.Background };
        page.Controls.Add(header);
        header.Controls.Add(NativeUi.Title("Bảng điều khiển"));
        var btnRefresh = NativeUi.SecondaryButton("Làm mới");
        btnRefresh.Dock = DockStyle.Right;
        btnRefresh.Width = 110;
        btnRefresh.Click += (_, _) => LoadData();
        header.Controls.Add(btnRefresh);

        var metrics = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 116,
            ColumnCount = 4,
            Padding = new Padding(0, 0, 0, 12)
        };
        for (int i = 0; i < 4; i++) metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        page.Controls.Add(metrics);

        metrics.Controls.Add(MetricCard("Tổng lượt khám hôm nay", _lblTotalToday), 0, 0);
        metrics.Controls.Add(MetricCard("Đã khám hôm nay", _lblCompletedToday), 1, 0);
        metrics.Controls.Add(MetricCard("Đang chờ hôm nay", _lblWaitingToday), 2, 0);
        metrics.Controls.Add(MetricCard("Lượt khám 7 ngày", _lblTotal7Days), 3, 0);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Background
        };
        NativeUi.ConfigureSplitter(split, desiredDistance: 620, panel1MinSize: 360, panel2MinSize: 300);
        page.Controls.Add(split);
        split.BringToFront();

        var statsCard = NativeUi.Card(DockStyle.Fill);
        split.Panel1.Controls.Add(statsCard);
        statsCard.Controls.Add(_gridStats);
        statsCard.Controls.Add(NativeUi.Section("Xu hướng khám bệnh 7 ngày qua"));

        var queueCard = NativeUi.Card(DockStyle.Fill);
        split.Panel2.Controls.Add(queueCard);
        queueCard.Controls.Add(_gridQueue);
        queueCard.Controls.Add(NativeUi.Section("Bệnh nhân đang chờ"));
    }

    private static Panel MetricCard(string title, Label value)
    {
        var card = NativeUi.Card(DockStyle.Fill);
        card.Margin = new Padding(0, 0, 10, 0);
        card.Controls.Add(value);
        card.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 24,
            Text = title,
            Font = UiTheme.LabelFont,
            ForeColor = UiTheme.MutedText,
            TextAlign = ContentAlignment.MiddleLeft
        });
        return card;
    }

    private void LoadData()
    {
        try
        {
            DataTable stats = _thongKeBLL.LayThongKe7Ngay();
            _gridStats.DataSource = stats;
            Header(_gridStats, "Ngay", "Ngày", 120);
            Header(_gridStats, "SoLuot", "Số lượt", 90);
            Header(_gridStats, "SoDaKham", "Đã khám", 90);
            Header(_gridStats, "SoDangCho", "Đang chờ", 90);

            int total7Days = 0;
            foreach (DataRow row in stats.Rows)
            {
                total7Days += NativeUi.IntOf(row, "SoLuot");
            }

            DataRow? today = stats.Rows.Count > 0 ? stats.Rows[0] : null;
            _lblTotalToday.Text = today == null ? "0" : NativeUi.TextOf(today, "SoLuot");
            _lblCompletedToday.Text = today == null ? "0" : NativeUi.TextOf(today, "SoDaKham");
            _lblWaitingToday.Text = today == null ? "0" : NativeUi.TextOf(today, "SoDangCho");
            _lblTotal7Days.Text = total7Days.ToString();

            DataTable queue = _khamBLL.LayHangDoiDangCho();
            _gridQueue.DataSource = queue;
            Header(_gridQueue, "SoThuTu", "STT", 60);
            Header(_gridQueue, "MaBN", "Mã BN", 70);
            Header(_gridQueue, "HoTen", "Họ tên", 180);
            Header(_gridQueue, "ThoiGianChoPhut", "Chờ (phút)", 90);
            Header(_gridQueue, "TrangThai", "Trạng thái", 100);
            if (_gridQueue.Columns.Contains("MaLK")) _gridQueue.Columns["MaLK"].Visible = false;
            if (_gridQueue.Columns.Contains("NgayKham")) _gridQueue.Columns["NgayKham"].Visible = false;
        }
        catch (Exception ex)
        {
            NativeUi.ShowError("Không tải được dashboard.\n" + ex.Message);
        }
    }

    private static void Header(DataGridView grid, string name, string text, int width)
    {
        if (!grid.Columns.Contains(name)) return;
        grid.Columns[name].HeaderText = text;
        grid.Columns[name].Width = width;
    }
}
