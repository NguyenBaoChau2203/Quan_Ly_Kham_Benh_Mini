using System.Data;
using System.Drawing.Printing;
using System.Text;
using ClinicApp.BLL;

namespace ClinicApp.GUI.Forms;

public class FrmInPhieu : Form
{
    private readonly KhamBLL _khamBLL = new();
    private readonly int? _maLK;
    private readonly NumericUpDown _numMaLK = new() { Minimum = 1, Maximum = 999999, Width = 120 };
    private readonly RichTextBox _preview = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        BorderStyle = BorderStyle.None,
        BackColor = Color.White,
        Font = new Font("Consolas", 10F),
        WordWrap = true
    };
    private readonly DataGridView _gridCompleted = NativeUi.Grid();
    private readonly Button _btnPrint = NativeUi.PrimaryButton("In phiếu");
    private string _printText = "";
    public event EventHandler? CloseRequested;

    public FrmInPhieu() : this((int?)null) { }
    public FrmInPhieu(int maLK) : this((int?)maLK) { }

    private FrmInPhieu(int? maLK)
    {
        _maLK = maLK > 0 ? maLK : null;
        UiTheme.ApplyForm(this);
        Text = "Xem trước phiếu khám";
        MinimumSize = new Size(760, 620);
        Size = new Size(860, 720);
        BuildLayout();
        Load += (_, _) =>
        {
            if (_maLK.HasValue)
            {
                _numMaLK.Value = _maLK.Value;
                LoadSlip();
            }
            else
            {
                _preview.Text = "Nhập mã lượt khám rồi bấm Tải phiếu để xem nội dung.";
                _btnPrint.Enabled = false;
            }
        };
    }

    private void BuildLayout()
    {
        var page = NativeUi.Page();
        Controls.Add(page);

        var toolbar = NativeUi.Card(DockStyle.Top, 62);
        page.Controls.Add(toolbar);

        var flow = NativeUi.Toolbar();
        flow.Controls.Add(NativeUi.FieldLabel("Mã lượt khám"));
        _numMaLK.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) LoadSlip(); };
        flow.Controls.Add(_numMaLK);

        var btnLoad = NativeUi.SecondaryButton("Tải phiếu");
        btnLoad.Width = 110;
        btnLoad.Click += (_, _) => LoadSlip();
        flow.Controls.Add(btnLoad);

        var btnCompleted = NativeUi.SecondaryButton("DS đã khám");
        btnCompleted.Width = 120;
        btnCompleted.Click += (_, _) => LoadCompletedList();
        flow.Controls.Add(btnCompleted);

        _btnPrint.Width = 100;
        _btnPrint.Click += (_, _) => PrintSlip();
        flow.Controls.Add(_btnPrint);

        var btnClose = NativeUi.SecondaryButton("Đóng");
        btnClose.Width = 90;
        btnClose.Click += (_, _) =>
        {
            if (CloseRequested is not null)
            {
                CloseRequested.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Close();
            }
        };
        flow.Controls.Add(btnClose);
        toolbar.Controls.Add(flow);

        var paperOuter = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.SurfaceContainerHigh,
            Padding = new Padding(36)
        };
        page.Controls.Add(paperOuter);
        paperOuter.BringToFront();

        var paper = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(28)
        };
        paperOuter.Controls.Add(paper);
        _gridCompleted.Visible = false;
        _gridCompleted.CellDoubleClick += (_, _) => LoadSlipFromSelectedRow();
        paper.Controls.Add(_gridCompleted);
        paper.Controls.Add(_preview);
    }

    private void LoadSlip()
    {
        try
        {
            DataTable table = _khamBLL.LayDuLieuInPhieu((int)_numMaLK.Value);
            if (table.Rows.Count == 0)
            {
                _printText = "";
                _preview.Text = "Chưa có dữ liệu phiếu khám cho lượt này.";
                _btnPrint.Enabled = false;
                return;
            }

            _printText = BuildSlipText(table.Rows[0]);
            _preview.Text = _printText;
            _gridCompleted.Visible = false;
            _preview.Visible = true;
            _preview.BringToFront();
            _btnPrint.Enabled = true;
        }
        catch (Exception ex)
        {
            _btnPrint.Enabled = false;
            NativeUi.ShowError("Lỗi khi tải phiếu khám.\n" + ex.Message);
        }
    }

    private static string BuildSlipText(DataRow row)
    {
        var sb = new StringBuilder();
        sb.AppendLine("                MINI CLINIC");
        sb.AppendLine("        PHIẾU KHÁM BỆNH");
        sb.AppendLine(new string('=', 52));
        sb.AppendLine($"Mã lượt khám : {NativeUi.TextOf(row, "MaLK")}");
        sb.AppendLine($"Số thứ tự    : {NativeUi.TextOf(row, "SoThuTu")}");
        sb.AppendLine($"Ngày khám    : {NativeUi.DateTimeText(row["NgayKham"])}");
        sb.AppendLine();
        sb.AppendLine($"Bệnh nhân    : {NativeUi.TextOf(row, "HoTen")}");
        sb.AppendLine($"Mã BN        : {NativeUi.TextOf(row, "MaBN")}");
        sb.AppendLine($"Ngày sinh    : {NativeUi.DateText(row["NgaySinh"])}");
        sb.AppendLine($"Giới tính    : {NativeUi.TextOf(row, "GioiTinh")}");
        sb.AppendLine($"Bác sĩ       : {NativeUi.TextOf(row, "TenBacSi")}");
        sb.AppendLine(new string('-', 52));
        AppendBlock(sb, "Triệu chứng", NativeUi.TextOf(row, "TrieuChung"));
        AppendBlock(sb, "Chẩn đoán", NativeUi.TextOf(row, "ChanDoan"));
        AppendBlock(sb, "Toa thuốc", NativeUi.TextOf(row, "ToaThuoc"));
        AppendBlock(sb, "Lời dặn", NativeUi.TextOf(row, "LoiDan"));
        sb.AppendLine(new string('-', 52));
        sb.AppendLine($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}");
        sb.AppendLine();
        sb.AppendLine("Vui lòng mang theo phiếu này khi vào phòng khám.");
        return sb.ToString();
    }

    private static void AppendBlock(StringBuilder sb, string title, string value)
    {
        sb.AppendLine($"{title}:");
        sb.AppendLine(string.IsNullOrWhiteSpace(value) ? "  Chưa có dữ liệu" : "  " + value.Replace("\n", "\n  "));
        sb.AppendLine();
    }

    private void PrintSlip()
    {
        if (string.IsNullOrWhiteSpace(_printText)) return;

        using var document = new PrintDocument();
        document.DocumentName = "PhieuKham";
        document.PrintPage += (_, e) =>
        {
            using var font = new Font("Consolas", 10F);
            e.Graphics.DrawString(_printText, font, Brushes.Black, e.MarginBounds);
        };

        using var dialog = new PrintDialog
        {
            Document = document,
            UseEXDialog = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            document.Print();
        }
    }

    private void LoadCompletedList()
    {
        try
        {
            DataTable table = _khamBLL.LayDanhSachDaKham();
            _gridCompleted.DataSource = table;
            ConfigureCompletedGrid();
            _preview.Visible = false;
            _gridCompleted.Visible = true;
            _gridCompleted.BringToFront();
            _btnPrint.Enabled = false;
            _printText = "";

            if (table.Rows.Count == 0)
            {
                NativeUi.ShowInfo("Chưa có lượt khám nào đã hoàn tất.");
            }
        }
        catch (Exception ex)
        {
            NativeUi.ShowError("Không tải được danh sách đã khám.\n" + ex.Message);
        }
    }

    private void LoadSlipFromSelectedRow()
    {
        if (_gridCompleted.CurrentRow?.DataBoundItem is not DataRowView view) return;
        int maLK = NativeUi.IntOf(view.Row, "MaLK");
        if (maLK <= 0) return;

        _numMaLK.Value = maLK;
        LoadSlip();
    }

    private void ConfigureCompletedGrid()
    {
        Header("MaLK", "Mã LK", 70);
        Header("SoThuTu", "STT", 60);
        Header("NgayKham", "Ngày khám", 150);
        Header("MaBN", "Mã BN", 70);
        Header("HoTen", "Họ tên", 190);
        Header("TenBacSi", "Bác sĩ", 160);
        Header("ChanDoan", "Chẩn đoán", 260);
    }

    private void Header(string name, string text, int width)
    {
        if (!_gridCompleted.Columns.Contains(name)) return;
        _gridCompleted.Columns[name].HeaderText = text;
        _gridCompleted.Columns[name].Width = width;
    }
}
