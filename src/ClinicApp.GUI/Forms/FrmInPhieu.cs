using System.Data;
using System.Globalization;
using System.Text;
using ClinicApp.BLL;
using ClinicApp.GUI;

namespace ClinicApp.GUI.Forms;

public class FrmInPhieu : PlaceholderForm
{
    private static readonly string[] RequiredColumns =
    {
        "MaLK",
        "SoThuTu",
        "NgayKham",
        "MaBN",
        "HoTen",
        "NgaySinh",
        "GioiTinh",
        "TenBacSi",
        "GhiChu",
        "TrieuChung",
        "ChanDoan",
        "ToaThuoc",
        "LoiDan"
    };

    private readonly KhamBLL _khamBLL = new();
    private readonly TextBox _txtMaLK = new();
    private readonly Button _btnLoad = new();
    private readonly Button _btnCopy = new();
    private readonly Button _btnClose = new();
    private readonly RichTextBox _previewBox = new();
    private readonly Label _statusLabel = new();
    private int? _maLK;

    public FrmInPhieu() : this((int?)null)
    {
    }

    public FrmInPhieu(int maLK) : this((int?)maLK)
    {
    }

    private FrmInPhieu(int? maLK) : base("Xem trước phiếu khám")
    {
        _maLK = maLK > 0 ? maLK : null;

        BodyPanel.Controls.Clear();
        BodyPanel.Padding = new Padding(16);
        BuildLayout();

        Load += (_, _) =>
        {
            // Hide close button when embedded (TopLevel=false) to avoid empty content panel
            if (!TopLevel) _btnClose.Visible = false;
            LoadPreview();
        };
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            BackColor = UiTheme.Surface,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 4
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

        root.Controls.Add(BuildLookupBar(), 0, 0);

        _previewBox.BackColor = Color.White;
        _previewBox.BorderStyle = BorderStyle.FixedSingle;
        _previewBox.DetectUrls = false;
        _previewBox.Dock = DockStyle.Fill;
        _previewBox.Font = new Font("Consolas", 10F, FontStyle.Regular);
        _previewBox.ReadOnly = true;
        _previewBox.ScrollBars = RichTextBoxScrollBars.Vertical;
        _previewBox.WordWrap = true;
        root.Controls.Add(_previewBox, 0, 1);

        root.Controls.Add(BuildActionBar(), 0, 2);

        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.Font = UiTheme.SmallFont;
        _statusLabel.ForeColor = UiTheme.MutedText;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        root.Controls.Add(_statusLabel, 0, 3);

        BodyPanel.Controls.Add(root);
    }

    private Control BuildLookupBar()
    {
        var lookup = new TableLayoutPanel
        {
            ColumnCount = 4,
            Dock = DockStyle.Fill,
            RowCount = 1
        };
        lookup.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        lookup.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        lookup.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
        lookup.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var label = new Label
        {
            Dock = DockStyle.Fill,
            Font = UiTheme.LabelFont,
            ForeColor = UiTheme.MutedText,
            Text = "Mã lượt khám",
            TextAlign = ContentAlignment.MiddleLeft
        };

        _txtMaLK.Dock = DockStyle.Fill;
        _txtMaLK.Margin = new Padding(0, 5, 8, 5);
        _txtMaLK.Text = _maLK?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        UiTheme.ApplyTextBox(_txtMaLK);
        _txtMaLK.KeyDown += TxtMaLK_KeyDown;

        _btnLoad.Text = "Tải";
        _btnLoad.Dock = DockStyle.Fill;
        _btnLoad.Margin = new Padding(0, 5, 8, 5);
        UiTheme.ApplySecondaryButton(_btnLoad);
        _btnLoad.Click += (_, _) => LoadRequestedPreview();

        lookup.Controls.Add(label, 0, 0);
        lookup.Controls.Add(_txtMaLK, 1, 0);
        lookup.Controls.Add(_btnLoad, 2, 0);
        lookup.Controls.Add(new Panel { BackColor = UiTheme.Surface, Dock = DockStyle.Fill }, 3, 0);

        return lookup;
    }

    private Control BuildActionBar()
    {
        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 6, 0, 0),
            WrapContents = false
        };

        _btnCopy.Text = "Sao chép nội dung";
        _btnCopy.Width = 142;
        UiTheme.ApplySecondaryButton(_btnCopy);
        _btnCopy.Click += CopyButton_Click;

        _btnClose.Text = "Đóng";
        _btnClose.Width = 92;
        UiTheme.ApplySecondaryButton(_btnClose);
        _btnClose.Click += (_, _) => Close();

        actions.Controls.Add(_btnCopy);
        actions.Controls.Add(_btnClose);

        return actions;
    }

    private void LoadRequestedPreview()
    {
        if (!int.TryParse(_txtMaLK.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int maLK) || maLK <= 0)
        {
            MessageBox.Show(
                "Vui lòng nhập mã lượt khám hợp lệ.",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        _maLK = maLK;
        LoadPreview();
    }

    private void LoadPreview()
    {
        if (_maLK is null)
        {
            ShowEmptyPreview("Nhập mã lượt khám để xem nội dung phiếu trên màn hình.");
            _btnCopy.Enabled = false;
            return;
        }

        _txtMaLK.Text = _maLK.Value.ToString(CultureInfo.InvariantCulture);

        DataTable table;
        try
        {
            table = _khamBLL.LayDuLieuInPhieu(_maLK.Value);
        }
        catch
        {
            table = new DataTable();
        }

        if (table.Rows.Count == 0)
        {
            ShowEmptyPreview("Chưa có dữ liệu phiếu khám cho lượt này. Khi Hung hoàn tất KhamDAL/KhamBLL, form sẽ tự render dữ liệu trả về từ LayDuLieuInPhieu.");
            _btnCopy.Enabled = true;
            return;
        }

        DataRow row = table.Rows[0];
        string[] missingColumns = RequiredColumns.Where(column => !table.Columns.Contains(column)).ToArray();
        _previewBox.Text = BuildPreviewText(row, missingColumns);
        _btnCopy.Enabled = true;

        SetStatus(missingColumns.Length == 0
            ? "Phiếu đang được render trực tiếp trên màn hình, không phụ thuộc máy in vật lý."
            : "Dữ liệu in phiếu đang thiếu cột: " + string.Join(", ", missingColumns));
    }

    private void ShowEmptyPreview(string message)
    {
        var builder = new StringBuilder();
        builder.AppendLine("PHIẾU KHÁM BỆNH");
        builder.AppendLine(new string('=', 72));
        builder.AppendLine();
        builder.AppendLine(message);
        builder.AppendLine();
        builder.AppendLine("Các cột bắt buộc từ KhamBLL.LayDuLieuInPhieu:");
        foreach (string column in RequiredColumns)
        {
            builder.AppendLine("- " + column);
        }

        _previewBox.Text = builder.ToString();
        SetStatus(message);
    }

    private static string BuildPreviewText(DataRow row, string[] missingColumns)
    {
        var builder = new StringBuilder();
        builder.AppendLine("PHIẾU KHÁM BỆNH");
        builder.AppendLine(new string('=', 72));
        builder.AppendLine($"Mã lượt khám : {Value(row, "MaLK")}");
        builder.AppendLine($"Số thứ tự    : {Value(row, "SoThuTu")}");
        builder.AppendLine($"Ngày khám    : {DateValue(row, "NgayKham", includeTime: true)}");
        builder.AppendLine();

        builder.AppendLine("I. THÔNG TIN BỆNH NHÂN");
        builder.AppendLine(new string('-', 72));
        builder.AppendLine($"Mã bệnh nhân : {Value(row, "MaBN")}");
        builder.AppendLine($"Họ tên       : {Value(row, "HoTen")}");
        builder.AppendLine($"Ngày sinh    : {DateValue(row, "NgaySinh", includeTime: false)}");
        builder.AppendLine($"Giới tính    : {Value(row, "GioiTinh")}");
        builder.AppendLine($"Bác sĩ       : {Value(row, "TenBacSi")}");
        builder.AppendLine($"Ghi chú      : {Value(row, "GhiChu")}");
        builder.AppendLine();

        builder.AppendLine("II. NỘI DUNG KHÁM");
        builder.AppendLine(new string('-', 72));
        AppendBlock(builder, "Triệu chứng", Value(row, "TrieuChung"));
        AppendBlock(builder, "Chẩn đoán", Value(row, "ChanDoan"));
        AppendBlock(builder, "Toa thuốc", Value(row, "ToaThuoc"));
        AppendBlock(builder, "Lời dặn", Value(row, "LoiDan"));

        builder.AppendLine();
        builder.AppendLine(new string('-', 72));
        builder.AppendLine("Phiếu này là bản xem trước trên màn hình phục vụ demo.");

        if (missingColumns.Length > 0)
        {
            builder.AppendLine();
            builder.AppendLine("Cảnh báo dữ liệu: thiếu cột " + string.Join(", ", missingColumns));
        }

        return builder.ToString();
    }

    private static void AppendBlock(StringBuilder builder, string title, string value)
    {
        builder.AppendLine(title + ":");
        builder.AppendLine(string.IsNullOrWhiteSpace(value) ? "  Chưa có dữ liệu" : IndentMultiline(value.Trim()));
        builder.AppendLine();
    }

    private static string IndentMultiline(string value)
    {
        string[] lines = value.Replace("\r\n", "\n").Split('\n');
        return string.Join(Environment.NewLine, lines.Select(line => "  " + line));
    }

    private void CopyButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_previewBox.Text))
        {
            return;
        }

        Clipboard.SetText(_previewBox.Text);
        SetStatus("Đã sao chép nội dung phiếu xem trước.");
    }

    private void TxtMaLK_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            LoadRequestedPreview();
        }
    }

    private void SetStatus(string message)
    {
        _statusLabel.Text = message;
    }

    private static string Value(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
        {
            return string.Empty;
        }

        return Convert.ToString(row[columnName], CultureInfo.CurrentCulture) ?? string.Empty;
    }

    private static string DateValue(DataRow row, string columnName, bool includeTime)
    {
        if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
        {
            return string.Empty;
        }

        object value = row[columnName];
        if (value is DateTime date)
        {
            return date.ToString(includeTime ? "dd/MM/yyyy HH:mm" : "dd/MM/yyyy", CultureInfo.CurrentCulture);
        }

        string text = Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty;
        return DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsed)
            ? parsed.ToString(includeTime ? "dd/MM/yyyy HH:mm" : "dd/MM/yyyy", CultureInfo.CurrentCulture)
            : text;
    }
}
