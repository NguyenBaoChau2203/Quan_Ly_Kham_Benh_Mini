using System.Data;
using System.Globalization;
using ClinicApp.BLL;
using ClinicApp.DTO;

namespace ClinicApp.GUI.Forms;

public class FrmKhamBenh : Form
{
    private readonly KhamBLL _khamBLL = new();
    private readonly int? _maBacSi;
    private readonly int? _maLK;
    private readonly DataRow? _visitSnapshot;

    private readonly Label _lblHoTen = InfoLabel();
    private readonly Label _lblMaBN = InfoLabel();
    private readonly Label _lblNgaySinh = InfoLabel();
    private readonly Label _lblGioiTinh = InfoLabel();
    private readonly Label _lblSTT = InfoLabel();
    private readonly Label _lblNgayKham = InfoLabel();
    private readonly Label _lblTenBacSi = InfoLabel();
    private readonly Label _lblStatus = new()
    {
        Dock = DockStyle.Fill,
        Font = UiTheme.LabelFont,
        ForeColor = UiTheme.MutedText,
        TextAlign = ContentAlignment.MiddleLeft
    };

    private readonly TextBox _txtTrieuChung = NativeUi.MultilineTextBox("Nhập triệu chứng...", 120);
    private readonly TextBox _txtChanDoan = NativeUi.MultilineTextBox("Nhập chẩn đoán...", 120);
    private readonly TextBox _txtToaThuoc = NativeUi.MultilineTextBox("Nhập toa thuốc...", 120);
    private readonly TextBox _txtLoiDan = NativeUi.MultilineTextBox("Lời dặn của bác sĩ...", 120);
    private readonly Button _btnSave = NativeUi.PrimaryButton("Lưu & xem phiếu");
    private readonly Label _emptyState = new()
    {
        Dock = DockStyle.Top,
        Height = 86,
        Font = UiTheme.SectionHeaderFont,
        ForeColor = UiTheme.Primary,
        BackColor = UiTheme.SecondaryContainer,
        Text = "Chưa chọn lượt khám.\nVào Hàng đợi khám, chọn một bệnh nhân rồi bấm Bắt đầu khám để mở màn hình này.",
        TextAlign = ContentAlignment.MiddleCenter,
        Visible = false
    };

    private bool _isSaved;
    public event EventHandler? BackRequested;

    public FrmKhamBenh() : this(null, null, null) { }
    public FrmKhamBenh(int maLK) : this(maLK, null, null) { }
    public FrmKhamBenh(int maLK, int? maBacSi) : this(maLK, maBacSi, null) { }
    public FrmKhamBenh(DataRow visitRow) : this(ReadNullableInt(visitRow, "MaLK"), null, visitRow) { }
    public FrmKhamBenh(DataRow visitRow, int? maBacSi) : this(ReadNullableInt(visitRow, "MaLK"), maBacSi, visitRow) { }
    public FrmKhamBenh(DataRowView visitRow, int? maBacSi) : this(visitRow.Row, maBacSi) { }

    private FrmKhamBenh(int? maLK, int? maBacSi, DataRow? visitSnapshot)
    {
        _maLK = maLK > 0 ? maLK : null;
        _maBacSi = maBacSi > 0 ? maBacSi : null;
        _visitSnapshot = visitSnapshot;

        UiTheme.ApplyForm(this);
        Text = "Khám bệnh";
        BuildLayout();
        Load += (_, _) => LoadData();
    }

    private static Label InfoLabel()
    {
        return new Label
        {
            AutoSize = false,
            Height = 22,
            Dock = DockStyle.Top,
            Font = UiTheme.BodyFont,
            ForeColor = UiTheme.Text,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private static int? ReadNullableInt(DataRow? row, string columnName)
    {
        if (row?.Table.Columns.Contains(columnName) != true) return null;
        object value = row[columnName];
        if (value is null || value == DBNull.Value) return null;
        return int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out int parsed) && parsed > 0 ? parsed : null;
    }

    private void BuildLayout()
    {
        var page = NativeUi.Page();
        Controls.Add(page);
        page.Controls.Add(_emptyState);

        var top = new SplitContainer
        {
            Dock = DockStyle.Top,
            Height = 132,
            FixedPanel = FixedPanel.Panel2,
            BackColor = UiTheme.Background
        };
        NativeUi.ConfigureSplitter(top, desiredDistance: 680, panel1MinSize: 420, panel2MinSize: 320);
        page.Controls.Add(top);

        var patientCard = NativeUi.Card(DockStyle.Fill);
        top.Panel1.Controls.Add(patientCard);
        patientCard.Controls.Add(BuildInfoGrid(("Họ và tên", _lblHoTen), ("Mã bệnh nhân", _lblMaBN), ("Ngày sinh", _lblNgaySinh), ("Giới tính", _lblGioiTinh)));
        patientCard.Controls.Add(NativeUi.Section("Thông tin bệnh nhân"));

        var visitCard = NativeUi.Card(DockStyle.Fill);
        top.Panel2.Controls.Add(visitCard);
        visitCard.Controls.Add(BuildInfoGrid(("Số thứ tự", _lblSTT), ("Ngày khám", _lblNgayKham), ("Bác sĩ", _lblTenBacSi)));
        visitCard.Controls.Add(NativeUi.Section("Thông tin lượt khám"));

        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = UiTheme.BodyFont
        };
        page.Controls.Add(tabs);
        tabs.BringToFront();

        var tabExam = new TabPage("Khám & Chẩn đoán") { BackColor = UiTheme.SurfaceContainerLowest, Padding = new Padding(12) };
        var tabTreatment = new TabPage("Điều trị & Toa thuốc") { BackColor = UiTheme.SurfaceContainerLowest, Padding = new Padding(12) };
        tabs.TabPages.Add(tabExam);
        tabs.TabPages.Add(tabTreatment);

        tabExam.Controls.Add(NativeUi.Field("Chẩn đoán bệnh *", _txtChanDoan));
        tabExam.Controls.Add(NativeUi.Field("Triệu chứng lâm sàng", _txtTrieuChung));
        tabTreatment.Controls.Add(NativeUi.Field("Lời dặn", _txtLoiDan));
        tabTreatment.Controls.Add(NativeUi.Field("Toa thuốc", _txtToaThuoc));

        var bottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            BackColor = UiTheme.SurfaceContainerLowest,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(12, 8, 12, 8)
        };
        page.Controls.Add(bottom);

        var btnBack = NativeUi.SecondaryButton("Quay lại");
        btnBack.Dock = DockStyle.Right;
        btnBack.Width = 110;
        btnBack.Click += (_, _) =>
        {
            if (BackRequested is not null)
            {
                BackRequested.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Close();
            }
        };

        _btnSave.Dock = DockStyle.Right;
        _btnSave.Width = 150;
        _btnSave.Margin = new Padding(8, 0, 0, 0);
        _btnSave.Click += (_, _) => SaveExam();

        bottom.Controls.Add(btnBack);
        bottom.Controls.Add(_btnSave);
        bottom.Controls.Add(_lblStatus);
    }

    private static Control BuildInfoGrid(params (string Label, Label Value)[] items)
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoScroll = false
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));

        foreach ((string label, Label value) in items)
        {
            int row = table.RowCount++;
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            table.Controls.Add(new Label
            {
                Text = label,
                Font = UiTheme.LabelFont,
                ForeColor = UiTheme.MutedText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, row);
            table.Controls.Add(value, 1, row);
        }

        return table;
    }

    private void LoadData()
    {
        if (_maLK is null or <= 0)
        {
            _lblStatus.Text = "Lượt khám chưa được chọn từ hàng đợi.";
            _emptyState.Visible = true;
            _emptyState.BringToFront();
            ClearVisitInfo();
            SetEditorEnabled(false);
            return;
        }

        _emptyState.Visible = false;

        try
        {
            DataTable table = _khamBLL.LayDuLieuInPhieu(_maLK.Value);
            DataRow? row = table.Rows.Count > 0 ? table.Rows[0] : _visitSnapshot;
            if (row == null)
            {
                _lblStatus.Text = "Chưa tải được thông tin lượt khám.";
                SetEditorEnabled(false);
                return;
            }

            _lblHoTen.Text = NativeUi.TextOf(row, "HoTen");
            _lblMaBN.Text = NativeUi.TextOf(row, "MaBN");
            _lblNgaySinh.Text = NativeUi.DateText(row["NgaySinh"]);
            _lblGioiTinh.Text = NativeUi.TextOf(row, "GioiTinh");
            _lblSTT.Text = NativeUi.TextOf(row, "SoThuTu");
            _lblNgayKham.Text = NativeUi.DateTimeText(row["NgayKham"]);
            _lblTenBacSi.Text = NativeUi.TextOf(row, "TenBacSi");

            _txtTrieuChung.Text = NativeUi.TextOf(row, "TrieuChung");
            _txtChanDoan.Text = NativeUi.TextOf(row, "ChanDoan");
            _txtToaThuoc.Text = NativeUi.TextOf(row, "ToaThuoc");
            _txtLoiDan.Text = NativeUi.TextOf(row, "LoiDan");

            _isSaved = !string.IsNullOrWhiteSpace(_txtChanDoan.Text) ||
                       !string.IsNullOrWhiteSpace(_txtTrieuChung.Text) ||
                       !string.IsNullOrWhiteSpace(_txtToaThuoc.Text) ||
                       !string.IsNullOrWhiteSpace(_txtLoiDan.Text);

            SetEditorEnabled(!_isSaved);
            _lblStatus.Text = _isSaved
                ? "Lượt khám đã có nội dung. Có thể xem phiếu."
                : "Sẵn sàng nhập khám. Chẩn đoán là bắt buộc.";
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Chưa tải được thông tin lượt khám.";
            NativeUi.ShowError(ex.Message);
        }
    }

    private void ClearVisitInfo()
    {
        foreach (Label label in new[] { _lblHoTen, _lblMaBN, _lblNgaySinh, _lblGioiTinh, _lblSTT, _lblNgayKham, _lblTenBacSi })
        {
            label.Text = "---";
        }

        foreach (TextBox box in new[] { _txtTrieuChung, _txtChanDoan, _txtToaThuoc, _txtLoiDan })
        {
            box.Clear();
        }
    }

    private void SetEditorEnabled(bool enabled)
    {
        foreach (TextBox box in new[] { _txtTrieuChung, _txtChanDoan, _txtToaThuoc, _txtLoiDan })
        {
            box.ReadOnly = !enabled;
            box.BackColor = enabled ? UiTheme.SurfaceContainerLowest : UiTheme.SurfaceContainerLow;
        }

        _btnSave.Text = _isSaved ? "Xem phiếu in" : "Lưu & xem phiếu";
        _btnSave.Enabled = enabled || _isSaved;
    }

    private void SaveExam()
    {
        if (_maLK is null or <= 0) return;

        if (_isSaved)
        {
            OpenPreview(_maLK.Value);
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtChanDoan.Text))
        {
            NativeUi.ShowError("Vui lòng nhập chẩn đoán trước khi hoàn tất khám.");
            _txtChanDoan.Focus();
            return;
        }

        var dto = new ChiTietKhamDTO
        {
            MaLK = _maLK.Value,
            TrieuChung = _txtTrieuChung.Text.Trim(),
            ChanDoan = _txtChanDoan.Text.Trim(),
            ToaThuoc = _txtToaThuoc.Text.Trim(),
            LoiDan = _txtLoiDan.Text.Trim()
        };

        try
        {
            if (!_khamBLL.HoanTatKham(dto))
            {
                NativeUi.ShowError("Không thể hoàn tất lượt khám.");
                return;
            }

            _isSaved = true;
            SetEditorEnabled(false);
            _lblStatus.Text = "Đã lưu lượt khám thành công.";
            OpenPreview(_maLK.Value);
        }
        catch (Exception ex)
        {
            NativeUi.ShowError("Đã xảy ra lỗi khi lưu.\n" + ex.Message);
        }
    }

    private void OpenPreview(int maLK)
    {
        using var preview = new FrmInPhieu(maLK);
        preview.ShowDialog(this);
    }
}
