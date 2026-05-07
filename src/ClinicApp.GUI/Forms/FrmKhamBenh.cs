using System.Data;
using System.Globalization;
using ClinicApp.BLL;
using ClinicApp.DTO;
using ClinicApp.GUI;

namespace ClinicApp.GUI.Forms;

public class FrmKhamBenh : PlaceholderForm
{
    private readonly KhamBLL _khamBLL = new();
    private readonly int? _maBacSi;
    private DataRow? _visitSnapshot;
    private int? _maLK;
    private bool _saved;

    private readonly TextBox _txtMaLK = new();
    private readonly TextBox _txtTrieuChung = new();
    private readonly TextBox _txtChanDoan = new();
    private readonly TextBox _txtToaThuoc = new();
    private readonly TextBox _txtLoiDan = new();
    private readonly Button _btnLoad = new();
    private readonly Button _btnSave = new();
    private readonly Button _btnPreview = new();
    private readonly Button _btnClose = new();
    private readonly Label _statusLabel = new();
    private readonly Dictionary<string, Label> _infoLabels = new(StringComparer.OrdinalIgnoreCase);

    public FrmKhamBenh() : this(null, null, null)
    {
    }

    public FrmKhamBenh(int maLK) : this(maLK, null, null)
    {
    }

    public FrmKhamBenh(int maLK, int? maBacSi) : this(maLK, maBacSi, null)
    {
    }

    public FrmKhamBenh(DataRow visitRow) : this(ReadNullableInt(visitRow, "MaLK"), null, visitRow)
    {
    }

    public FrmKhamBenh(DataRow visitRow, int? maBacSi) : this(ReadNullableInt(visitRow, "MaLK"), maBacSi, visitRow)
    {
    }

    public FrmKhamBenh(DataRowView visitRow, int? maBacSi) : this(visitRow.Row, maBacSi)
    {
    }

    private FrmKhamBenh(int? maLK, int? maBacSi, DataRow? visitSnapshot) : base("Khám bệnh")
    {
        _maLK = maLK > 0 ? maLK : null;
        _maBacSi = maBacSi > 0 ? maBacSi : null;
        _visitSnapshot = visitSnapshot;

        BodyPanel.Controls.Clear();
        BodyPanel.Padding = new Padding(16);
        BuildLayout();

        Load += (_, _) => LoadVisitState();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            BackColor = UiTheme.Surface,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 5
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 170F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

        root.Controls.Add(BuildLookupBar(), 0, 0);
        root.Controls.Add(BuildInfoPanel(), 0, 1);
        root.Controls.Add(BuildEditorPanel(), 0, 2);
        root.Controls.Add(BuildActionBar(), 0, 3);

        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.Font = UiTheme.SmallFont;
        _statusLabel.ForeColor = UiTheme.MutedText;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        root.Controls.Add(_statusLabel, 0, 4);

        BodyPanel.Controls.Add(root);
    }

    private Control BuildLookupBar()
    {
        var lookup = new TableLayoutPanel
        {
            ColumnCount = 5,
            Dock = DockStyle.Fill,
            RowCount = 1
        };
        lookup.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        lookup.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));
        lookup.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
        lookup.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        lookup.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150F));

        var label = CreateInlineLabel("Mã lượt khám");
        _txtMaLK.Dock = DockStyle.Fill;
        _txtMaLK.Margin = new Padding(0, 5, 8, 5);
        _txtMaLK.Text = _maLK?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        UiTheme.ApplyTextBox(_txtMaLK);
        _txtMaLK.KeyDown += TxtMaLK_KeyDown;

        _btnLoad.Text = "Tải";
        _btnLoad.Dock = DockStyle.Fill;
        _btnLoad.Margin = new Padding(0, 5, 8, 5);
        UiTheme.ApplySecondaryButton(_btnLoad);
        _btnLoad.Click += (_, _) => LoadRequestedVisit();

        _btnPreview.Text = "Xem phiếu";
        _btnPreview.Dock = DockStyle.Fill;
        _btnPreview.Margin = new Padding(8, 5, 0, 5);
        UiTheme.ApplySecondaryButton(_btnPreview);
        _btnPreview.Click += (_, _) => OpenPreview();

        lookup.Controls.Add(label, 0, 0);
        lookup.Controls.Add(_txtMaLK, 1, 0);
        lookup.Controls.Add(_btnLoad, 2, 0);
        lookup.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface }, 3, 0);
        lookup.Controls.Add(_btnPreview, 4, 0);

        return lookup;
    }

    private Control BuildInfoPanel()
    {
        var group = new GroupBox
        {
            Dock = DockStyle.Fill,
            Font = UiTheme.SectionHeaderFont,
            ForeColor = UiTheme.Text,
            Padding = new Padding(10, 18, 10, 8),
            Text = "Thông tin lượt khám"
        };

        var grid = new TableLayoutPanel
        {
            ColumnCount = 4,
            Dock = DockStyle.Fill,
            RowCount = 5
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

        for (int i = 0; i < 5; i++)
        {
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));
        }

        AddInfoRow(grid, 0, "Mã lượt", "MaLK", "Số thứ tự", "SoThuTu");
        AddInfoRow(grid, 1, "Mã bệnh nhân", "MaBN", "Họ tên", "HoTen");
        AddInfoRow(grid, 2, "Ngày sinh", "NgaySinh", "Giới tính", "GioiTinh");
        AddInfoRow(grid, 3, "Ngày khám", "NgayKham", "Bác sĩ", "TenBacSi");
        AddInfoRow(grid, 4, "Ghi chú", "GhiChu", "Trạng thái", "TrangThai");

        group.Controls.Add(grid);
        return group;
    }

    private Control BuildEditorPanel()
    {
        var editor = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            RowCount = 4,
            Padding = new Padding(0, 10, 0, 0)
        };
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132F));
        editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        for (int i = 0; i < 4; i++)
        {
            editor.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
        }

        ConfigureMultilineTextBox(_txtTrieuChung);
        ConfigureMultilineTextBox(_txtChanDoan);
        ConfigureMultilineTextBox(_txtToaThuoc);
        ConfigureMultilineTextBox(_txtLoiDan);

        AddEditorRow(editor, 0, "Triệu chứng", _txtTrieuChung);
        AddEditorRow(editor, 1, "Chẩn đoán (*)", _txtChanDoan);
        AddEditorRow(editor, 2, "Toa thuốc", _txtToaThuoc);
        AddEditorRow(editor, 3, "Lời dặn", _txtLoiDan);

        return editor;
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

        _btnSave.Text = "Lưu và xem phiếu";
        _btnSave.Width = 150;
        UiTheme.ApplyPrimaryButton(_btnSave);
        _btnSave.Click += SaveButton_Click;

        _btnClose.Text = "Đóng";
        _btnClose.Width = 92;
        UiTheme.ApplySecondaryButton(_btnClose);
        _btnClose.Click += (_, _) => Close();

        actions.Controls.Add(_btnSave);
        actions.Controls.Add(_btnClose);

        return actions;
    }

    private void LoadRequestedVisit()
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
        _visitSnapshot = null;
        _saved = false;
        LoadVisitState();
    }

    private void LoadVisitState(bool keepCurrentExamText = false)
    {
        string trieuChung = _txtTrieuChung.Text;
        string chanDoan = _txtChanDoan.Text;
        string toaThuoc = _txtToaThuoc.Text;
        string loiDan = _txtLoiDan.Text;

        if (_maLK is null)
        {
            BindVisitInfo(null);
            ClearExamText();
            SetEditorEnabled(false);
            _btnSave.Enabled = false;
            _btnPreview.Enabled = false;
            SetStatus("Nhập mã lượt khám hoặc mở form từ hàng đợi để bắt đầu khám.");
            return;
        }

        _txtMaLK.Text = _maLK.Value.ToString(CultureInfo.InvariantCulture);

        DataRow? row = _visitSnapshot;
        try
        {
            DataTable table = _khamBLL.LayDuLieuInPhieu(_maLK.Value);
            if (table.Rows.Count > 0)
            {
                row = table.Rows[0];
            }
        }
        catch
        {
            row = _visitSnapshot;
        }

        BindVisitInfo(row);
        BindExamText(row);

        if (keepCurrentExamText && !HasMedicalText(row))
        {
            _txtTrieuChung.Text = trieuChung;
            _txtChanDoan.Text = chanDoan;
            _txtToaThuoc.Text = toaThuoc;
            _txtLoiDan.Text = loiDan;
        }

        _saved = _saved || HasMedicalText(row);
        SetEditorEnabled(!_saved);
        _btnSave.Enabled = !_saved;
        _btnPreview.Enabled = true;

        if (row is null)
        {
            SetStatus("Chưa tải được thông tin lượt khám từ BLL/DAL. UI vẫn chỉ gọi KhamBLL và sẽ hiển thị dữ liệu khi Hung hoàn tất DAL/BLL.");
            return;
        }

        SetStatus(_saved
            ? "Lượt khám đã có nội dung khám. Có thể xem phiếu trên màn hình."
            : "Sẵn sàng nhập khám. Chẩn đoán là thông tin bắt buộc trước khi lưu.");
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        if (_maLK is null)
        {
            LoadRequestedVisit();
            if (_maLK is null)
            {
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(_txtChanDoan.Text))
        {
            MessageBox.Show(
                "Vui lòng nhập chẩn đoán trước khi hoàn tất khám.",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            _btnSave.Enabled = true;
            return;
        }

        _btnSave.Enabled = false;
        SetStatus("Đang lưu lượt khám...");

        var chiTiet = new ChiTietKhamDTO
        {
            MaLK = _maLK.Value,
            TrieuChung = _txtTrieuChung.Text.Trim(),
            ChanDoan = _txtChanDoan.Text.Trim(),
            ToaThuoc = _txtToaThuoc.Text.Trim(),
            LoiDan = _txtLoiDan.Text.Trim()
        };

        bool success;
        try
        {
            success = _khamBLL.HoanTatKham(chiTiet);
        }
        catch
        {
            success = false;
        }

        if (!success)
        {
            MessageBox.Show(
                "Không thể hoàn tất lượt khám. Lượt khám có thể đã được lưu, không còn ở trạng thái đang khám, hoặc dữ liệu bác sĩ chưa sẵn sàng. Màn hình sẽ được tải lại.",
                "Chưa lưu được",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            _saved = false;
            LoadVisitState(keepCurrentExamText: true);
            return;
        }

        _saved = true;
        SetEditorEnabled(false);
        _btnSave.Enabled = false;
        _btnPreview.Enabled = true;
        SetStatus("Đã lưu lượt khám. Đang mở phiếu xem trước trên màn hình.");
        OpenPreview();
    }

    private void OpenPreview()
    {
        if (_maLK is null)
        {
            MessageBox.Show(
                "Vui lòng nhập hoặc tải mã lượt khám trước khi xem phiếu.",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        using var preview = new FrmInPhieu(_maLK.Value);
        preview.ShowDialog(this);
    }

    private void BindVisitInfo(DataRow? row)
    {
        SetInfo("MaLK", _maLK?.ToString(CultureInfo.InvariantCulture) ?? GetCell(row, "MaLK"));
        SetInfo("SoThuTu", GetCell(row, "SoThuTu"));
        SetInfo("MaBN", GetCell(row, "MaBN"));
        SetInfo("HoTen", GetCell(row, "HoTen"));
        SetInfo("NgaySinh", FormatDate(GetValue(row, "NgaySinh"), includeTime: false));
        SetInfo("GioiTinh", GetCell(row, "GioiTinh"));
        SetInfo("NgayKham", FormatDate(GetValue(row, "NgayKham"), includeTime: true));
        string tenBacSi = GetCell(row, "TenBacSi");
        SetInfo("TenBacSi", string.IsNullOrWhiteSpace(tenBacSi) && _maBacSi.HasValue
            ? "Mã bác sĩ " + _maBacSi.Value.ToString(CultureInfo.InvariantCulture)
            : tenBacSi);
        SetInfo("GhiChu", GetCell(row, "GhiChu"));
        SetInfo("TrangThai", DisplayStatus(GetCell(row, "TrangThai")));
    }

    private void BindExamText(DataRow? row)
    {
        _txtTrieuChung.Text = GetCell(row, "TrieuChung");
        _txtChanDoan.Text = GetCell(row, "ChanDoan");
        _txtToaThuoc.Text = GetCell(row, "ToaThuoc");
        _txtLoiDan.Text = GetCell(row, "LoiDan");
    }

    private void ClearExamText()
    {
        _txtTrieuChung.Clear();
        _txtChanDoan.Clear();
        _txtToaThuoc.Clear();
        _txtLoiDan.Clear();
    }

    private void SetEditorEnabled(bool enabled)
    {
        _txtTrieuChung.ReadOnly = !enabled;
        _txtChanDoan.ReadOnly = !enabled;
        _txtToaThuoc.ReadOnly = !enabled;
        _txtLoiDan.ReadOnly = !enabled;
    }

    private void SetStatus(string message)
    {
        _statusLabel.Text = message;
    }

    private void SetInfo(string key, string value)
    {
        if (_infoLabels.TryGetValue(key, out Label? label))
        {
            label.Text = string.IsNullOrWhiteSpace(value) ? "Chưa có dữ liệu" : value;
        }
    }

    private void TxtMaLK_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            LoadRequestedVisit();
        }
    }

    private void AddInfoRow(TableLayoutPanel grid, int rowIndex, string leftLabel, string leftKey, string rightLabel, string rightKey)
    {
        AddInfoCell(grid, rowIndex, 0, leftLabel, leftKey);
        AddInfoCell(grid, rowIndex, 2, rightLabel, rightKey);
    }

    private void AddInfoCell(TableLayoutPanel grid, int rowIndex, int columnIndex, string labelText, string key)
    {
        var label = new Label
        {
            Dock = DockStyle.Fill,
            Font = UiTheme.LabelFont,
            ForeColor = UiTheme.MutedText,
            Text = labelText,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var value = new Label
        {
            Dock = DockStyle.Fill,
            Font = UiTheme.BodyFont,
            ForeColor = UiTheme.Text,
            Text = "Chưa có dữ liệu",
            TextAlign = ContentAlignment.MiddleLeft
        };

        _infoLabels[key] = value;
        grid.Controls.Add(label, columnIndex, rowIndex);
        grid.Controls.Add(value, columnIndex + 1, rowIndex);
    }

    private static void AddEditorRow(TableLayoutPanel editor, int rowIndex, string labelText, TextBox textBox)
    {
        var label = new Label
        {
            Dock = DockStyle.Fill,
            Font = UiTheme.LabelFont,
            ForeColor = UiTheme.MutedText,
            Text = labelText,
            TextAlign = ContentAlignment.TopLeft,
            Padding = new Padding(0, 7, 12, 0)
        };

        editor.Controls.Add(label, 0, rowIndex);
        editor.Controls.Add(textBox, 1, rowIndex);
    }

    private static Label CreateInlineLabel(string text)
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Font = UiTheme.LabelFont,
            ForeColor = UiTheme.MutedText,
            Text = text,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static void ConfigureMultilineTextBox(TextBox textBox)
    {
        textBox.AcceptsReturn = true;
        textBox.AcceptsTab = true;
        textBox.Dock = DockStyle.Fill;
        textBox.Multiline = true;
        textBox.ScrollBars = ScrollBars.Vertical;
        textBox.Margin = new Padding(0, 4, 0, 6);
        UiTheme.ApplyTextBox(textBox);
    }

    private static bool HasMedicalText(DataRow? row)
    {
        if (row is null)
        {
            return false;
        }

        return !string.IsNullOrWhiteSpace(GetCell(row, "TrieuChung"))
            || !string.IsNullOrWhiteSpace(GetCell(row, "ChanDoan"))
            || !string.IsNullOrWhiteSpace(GetCell(row, "ToaThuoc"))
            || !string.IsNullOrWhiteSpace(GetCell(row, "LoiDan"));
    }

    private static string GetCell(DataRow? row, string columnName)
    {
        object? value = GetValue(row, columnName);
        return value is null || value == DBNull.Value
            ? string.Empty
            : Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty;
    }

    private static object? GetValue(DataRow? row, string columnName)
    {
        if (row?.Table.Columns.Contains(columnName) != true)
        {
            return null;
        }

        return row[columnName];
    }

    private static int? ReadNullableInt(DataRow? row, string columnName)
    {
        string value = GetCell(row, columnName);
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) && parsed > 0
            ? parsed
            : null;
    }

    private static string FormatDate(object? value, bool includeTime)
    {
        if (value is null || value == DBNull.Value)
        {
            return string.Empty;
        }

        if (value is DateTime date)
        {
            return date.ToString(includeTime ? "dd/MM/yyyy HH:mm" : "dd/MM/yyyy", CultureInfo.CurrentCulture);
        }

        string text = Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty;
        return DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime parsed)
            ? parsed.ToString(includeTime ? "dd/MM/yyyy HH:mm" : "dd/MM/yyyy", CultureInfo.CurrentCulture)
            : text;
    }

    private static string DisplayStatus(string status)
    {
        return status switch
        {
            "DangCho" => "Đang chờ",
            "DangKham" => "Đang khám",
            "DaKham" => "Đã khám",
            "DaHuy" => "Đã hủy",
            _ => status
        };
    }
}
