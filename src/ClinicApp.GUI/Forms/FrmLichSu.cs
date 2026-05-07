#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using ClinicApp.BLL;

namespace ClinicApp.GUI.Forms;

public class FrmLichSu : Form
{
    private readonly ThongKeBLL _thongKeBLL = new();

    private DateTimePicker dtpFrom;
    private DateTimePicker dtpTo;
    private TextBox txtKeyword;
    private Button btnSearch;
    private DataGridView dgvLichSu;

    public FrmLichSu()
    {
        UiTheme.ApplyForm(this);
        Text = "Lịch sử khám bệnh";
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
            Text = "Lịch sử khám bệnh",
            TextAlign = ContentAlignment.MiddleLeft
        };
        header.Controls.Add(title);

        var filterBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 44,
            BackColor = UiTheme.Background
        };

        var lblFrom = new Label
        {
            Text = "Từ ngày:",
            AutoSize = true,
            Location = new Point(0, 12),
            Font = UiTheme.BodyFont,
            ForeColor = UiTheme.Text
        };

        dtpFrom = new DateTimePicker
        {
            Location = new Point(64, 8),
            Width = 130,
            Format = DateTimePickerFormat.Short,
            Font = UiTheme.BodyFont
        };
        dtpFrom.Value = DateTime.Today.AddDays(-7);

        var lblTo = new Label
        {
            Text = "Đến ngày:",
            AutoSize = true,
            Location = new Point(206, 12),
            Font = UiTheme.BodyFont,
            ForeColor = UiTheme.Text
        };

        dtpTo = new DateTimePicker
        {
            Location = new Point(274, 8),
            Width = 130,
            Format = DateTimePickerFormat.Short,
            Font = UiTheme.BodyFont
        };

        var lblKeyword = new Label
        {
            Text = "Từ khóa:",
            AutoSize = true,
            Location = new Point(416, 12),
            Font = UiTheme.BodyFont,
            ForeColor = UiTheme.Text
        };

        txtKeyword = new TextBox
        {
            Location = new Point(474, 8),
            Width = 180,
            Font = UiTheme.BodyFont,
            PlaceholderText = "Tên, SĐT, CCCD..."
        };

        btnSearch = new Button
        {
            Text = "Tìm",
            Location = new Point(662, 7),
            Width = 72
        };
        UiTheme.ApplyPrimaryButton(btnSearch);
        btnSearch.Click += BtnSearch_Click;

        filterBar.Controls.AddRange(new Control[]
        {
            lblFrom, dtpFrom, lblTo, dtpTo, lblKeyword, txtKeyword, btnSearch
        });

        var gridPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Surface,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(0)
        };

        dgvLichSu = new DataGridView
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

        gridPanel.Controls.Add(dgvLichSu);

        Controls.Add(gridPanel);
        Controls.Add(filterBar);
        Controls.Add(header);

        dgvLichSu.CellFormatting += DgvLichSu_CellFormatting;
    }

    private void BtnSearch_Click(object? sender, EventArgs e)
    {
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            DataTable dt = _thongKeBLL.LayLichSuKham(
                dtpFrom.Value.Date,
                dtpTo.Value.Date,
                txtKeyword.Text);

            dgvLichSu.DataSource = null;
            dgvLichSu.DataSource = dt;

            if (dgvLichSu.Columns.Count > 0)
            {
                if (dgvLichSu.Columns.Contains("MaLK")) { dgvLichSu.Columns["MaLK"]!.HeaderText = "Mã LK"; dgvLichSu.Columns["MaLK"]!.Visible = false; }
                if (dgvLichSu.Columns.Contains("NgayKham")) { dgvLichSu.Columns["NgayKham"]!.HeaderText = "Ngày khám"; dgvLichSu.Columns["NgayKham"]!.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm"; }
                if (dgvLichSu.Columns.Contains("MaBN")) { dgvLichSu.Columns["MaBN"]!.HeaderText = "Mã BN"; dgvLichSu.Columns["MaBN"]!.Visible = false; }
                if (dgvLichSu.Columns.Contains("HoTen")) dgvLichSu.Columns["HoTen"]!.HeaderText = "Họ tên";
                if (dgvLichSu.Columns.Contains("TenBacSi")) dgvLichSu.Columns["TenBacSi"]!.HeaderText = "Bác sĩ";
                if (dgvLichSu.Columns.Contains("ChanDoan")) dgvLichSu.Columns["ChanDoan"]!.HeaderText = "Chẩn đoán";
                if (dgvLichSu.Columns.Contains("TrangThai")) dgvLichSu.Columns["TrangThai"]!.HeaderText = "Trạng thái";
            }
        }
        catch
        {
            MessageBox.Show("Đã xảy ra lỗi khi tải lịch sử khám.", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DgvLichSu_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (dgvLichSu.Columns[e.ColumnIndex].Name == "TrangThai" && e.Value is string status)
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
}
