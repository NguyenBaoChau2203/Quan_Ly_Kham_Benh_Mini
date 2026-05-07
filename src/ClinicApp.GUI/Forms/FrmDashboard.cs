#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using ClinicApp.BLL;

namespace ClinicApp.GUI.Forms;

public class FrmDashboard : Form
{
    private readonly ThongKeBLL _thongKeBLL = new();

    private Label lblTotalVisits;
    private Label lblCompleted;
    private Label lblWaiting;
    private DataGridView dgvStats;

    public FrmDashboard()
    {
        UiTheme.ApplyForm(this);
        Text = "Bảng điều khiển";
        Padding = new Padding(16);

        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            BackColor = UiTheme.Background
        };

        var title = new Label
        {
            Dock = DockStyle.Fill,
            Font = UiTheme.ScreenHeaderFont,
            ForeColor = UiTheme.Text,
            Text = "Bảng điều khiển",
            TextAlign = ContentAlignment.MiddleLeft
        };
        header.Controls.Add(title);

        var summaryPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 100,
            BackColor = UiTheme.Background,
            Padding = new Padding(0, 8, 0, 8)
        };

        lblTotalVisits = CreateSummaryCard("Tổng lượt khám", summaryPanel, 0);
        lblCompleted = CreateSummaryCard("Đã khám", summaryPanel, 1);
        lblWaiting = CreateSummaryCard("Đang chờ", summaryPanel, 2);

        var gridPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(0)
        };

        dgvStats = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            BackgroundColor = Color.White,
            RowHeadersVisible = false,
            Font = UiTheme.BodyFont,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Font = UiTheme.LabelFont,
                BackColor = UiTheme.SurfaceLow,
                ForeColor = UiTheme.Text
            }
        };

        gridPanel.Controls.Add(dgvStats);

        Controls.Add(gridPanel);
        Controls.Add(summaryPanel);
        Controls.Add(header);
    }

    private Label CreateSummaryCard(string subtitle, Panel parent, int columnIndex)
    {
        int parentWidth = parent.ClientSize.Width;
        int cardWidth = parentWidth > 0 ? parentWidth / 3 : 220;
        int x = columnIndex * cardWidth;

        var card = new Panel
        {
            BackColor = UiTheme.Surface,
            BorderStyle = BorderStyle.FixedSingle,
            Location = new Point(x + 4, 4),
            Size = new Size(Math.Max(180, cardWidth - 8), 80)
        };

        var subLabel = new Label
        {
            AutoSize = true,
            Location = new Point(16, 12),
            Font = UiTheme.SmallFont,
            ForeColor = UiTheme.MutedText,
            Text = subtitle
        };

        var valueLabel = new Label
        {
            AutoSize = true,
            Location = new Point(16, 34),
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = UiTheme.Text,
            Text = "--"
        };

        card.Controls.Add(valueLabel);
        card.Controls.Add(subLabel);
        card.Resize += (_, _) =>
        {
            int width = parent.ClientSize.Width > 0 ? parent.ClientSize.Width / 3 : card.Width;
            card.Width = Math.Max(180, width - 8);
            card.Left = columnIndex * width;
        };

        parent.Controls.Add(card);

        return valueLabel;
    }

    private void LoadData()
    {
        try
        {
            DataTable dt = _thongKeBLL.LayThongKe7Ngay();

            if (!dt.Columns.Contains("SoLuot") || !dt.Columns.Contains("SoDaKham") || !dt.Columns.Contains("SoDangCho"))
            {
                MessageBox.Show("Dữ liệu thống kê thiếu cột bắt buộc. Vui lòng kiểm tra lại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int totalVisits = 0;
            int totalCompleted = 0;
            int totalWaiting = 0;

            foreach (DataRow row in dt.Rows)
            {
                totalVisits += Convert.ToInt32(row["SoLuot"]);
                totalCompleted += Convert.ToInt32(row["SoDaKham"]);
                totalWaiting += Convert.ToInt32(row["SoDangCho"]);
            }

            lblTotalVisits.Text = totalVisits.ToString();
            lblCompleted.Text = totalCompleted.ToString();
            lblWaiting.Text = totalWaiting.ToString();

            dgvStats.DataSource = null;
            dgvStats.DataSource = dt;

            if (dgvStats.Columns.Contains("Ngay"))
            {
                dgvStats.Columns["Ngay"]!.HeaderText = "Ngày";
                dgvStats.Columns["Ngay"]!.DefaultCellStyle.Format = "dd/MM/yyyy";
            }
            if (dgvStats.Columns.Contains("SoLuot")) dgvStats.Columns["SoLuot"]!.HeaderText = "Số lượt";
            if (dgvStats.Columns.Contains("SoDaKham")) dgvStats.Columns["SoDaKham"]!.HeaderText = "Đã khám";
            if (dgvStats.Columns.Contains("SoDangCho")) dgvStats.Columns["SoDangCho"]!.HeaderText = "Đang chờ";
        }
        catch
        {
            MessageBox.Show("Đã xảy ra lỗi khi tải dữ liệu thống kê.", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
