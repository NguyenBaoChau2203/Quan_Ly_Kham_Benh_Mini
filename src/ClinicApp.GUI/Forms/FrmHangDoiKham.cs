using System.Data;
using System.Drawing;
using System.Windows.Forms;
using ClinicApp.BLL;
using ClinicApp.DTO;
using ClinicApp.GUI;

namespace ClinicApp.GUI.Forms;

public class FrmHangDoiKham : PlaceholderForm
{
    private readonly KhamBLL _khamBLL = new();
    private readonly NhanVienDTO? _currentUser;
    private readonly DataGridView _grid = new();
    private readonly Button _refreshButton = new();
    private readonly Button _startExamButton = new();
    private readonly Label _statusLabel = new();
    private readonly Label _selectedLabel = new();
    private bool _isLoading;

    public event EventHandler<ExamStartedEventArgs>? ExamStarted;

    public FrmHangDoiKham(NhanVienDTO? currentUser) : base("Danh sách hàng đợi khám")
    {
        _currentUser = currentUser;
        BuildLayout();
        Load += (_, _) => RefreshQueue();
    }

    private void BuildLayout()
    {
        BodyPanel.SuspendLayout();
        BodyPanel.Controls.Clear();

        _grid.Dock = DockStyle.Fill;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.AutoGenerateColumns = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.BackgroundColor = UiTheme.Surface;
        _grid.BorderStyle = BorderStyle.None;
        _grid.ColumnHeadersHeight = 34;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _grid.MultiSelect = false;
        _grid.ReadOnly = true;
        _grid.RowHeadersVisible = false;
        _grid.RowTemplate.Height = 32;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.StandardTab = true;
        _grid.CellDoubleClick += (_, _) => StartSelectedExam();
        _grid.CellFormatting += Grid_CellFormatting;
        _grid.SelectionChanged += (_, _) => UpdateSelectionState();

        AddGridColumn("MaLK", "Mã lượt", 70F);
        AddGridColumn("SoThuTu", "STT", 58F);
        AddGridColumn("HoTen", "Bệnh nhân", 190F);
        AddGridColumn("MaBN", "Mã BN", 70F);
        AddGridColumn("NgayKham", "Giờ đăng ký", 130F, "dd/MM/yyyy HH:mm");
        AddGridColumn("ThoiGianChoPhut", "Chờ (phút)", 86F);
        AddGridColumn("TrangThai", "Trạng thái", 92F);

        _refreshButton.Text = "Làm mới";
        _refreshButton.Width = 112;
        UiTheme.ApplySecondaryButton(_refreshButton);
        _refreshButton.Click += (_, _) => RefreshQueue();

        _startExamButton.Text = "Bắt đầu khám";
        _startExamButton.Width = 136;
        UiTheme.ApplyPrimaryButton(_startExamButton);
        _startExamButton.Click += (_, _) => StartSelectedExam();

        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.Font = UiTheme.BodyFont;
        _statusLabel.ForeColor = UiTheme.MutedText;
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;

        _selectedLabel.Dock = DockStyle.Fill;
        _selectedLabel.Font = UiTheme.BodyFont;
        _selectedLabel.ForeColor = UiTheme.MutedText;
        _selectedLabel.TextAlign = ContentAlignment.MiddleLeft;

        var toolbar = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 48,
            ColumnCount = 3,
            BackColor = UiTheme.Surface,
            Padding = new Padding(0, 0, 0, 12)
        };
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 124F));
        toolbar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 148F));
        toolbar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        toolbar.Controls.Add(_statusLabel, 0, 0);
        toolbar.Controls.Add(_refreshButton, 1, 0);
        toolbar.Controls.Add(_startExamButton, 2, 0);

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 42,
            BackColor = UiTheme.Surface,
            Padding = new Padding(0, 10, 0, 0)
        };
        footer.Controls.Add(_selectedLabel);

        BodyPanel.Controls.Add(_grid);
        BodyPanel.Controls.Add(footer);
        BodyPanel.Controls.Add(toolbar);
        BodyPanel.ResumeLayout();

        UpdateSelectionState();
    }

    private void AddGridColumn(string name, string headerText, float fillWeight, string? format = null)
    {
        var column = new DataGridViewTextBoxColumn
        {
            Name = name,
            DataPropertyName = name,
            HeaderText = headerText,
            FillWeight = fillWeight,
            SortMode = DataGridViewColumnSortMode.Automatic
        };

        if (!string.IsNullOrWhiteSpace(format))
        {
            column.DefaultCellStyle.Format = format;
        }

        _grid.Columns.Add(column);
    }

    private void RefreshQueue(int? maLKToSelect = null)
    {
        if (_isLoading)
        {
            return;
        }

        int? selectedMaLK = maLKToSelect ?? GetSelectedMaLK();
        _isLoading = true;
        _refreshButton.Enabled = false;
        _startExamButton.Enabled = false;
        _statusLabel.Text = "Đang tải hàng đợi khám...";

        try
        {
            DataTable queue = _khamBLL.LayHangDoiDangCho() ?? new DataTable();
            _grid.DataSource = queue;
            RestoreSelection(selectedMaLK);

            _statusLabel.Text = queue.Rows.Count == 0
                ? "Chưa có lượt khám đang chờ."
                : $"Có {queue.Rows.Count} lượt khám đang chờ.";
        }
        catch
        {
            _grid.DataSource = new DataTable();
            _statusLabel.Text = "Không thể tải hàng đợi khám.";
            MessageBox.Show(
                "Không thể tải hàng đợi khám. Vui lòng thử lại sau.",
                "Hàng đợi khám",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
        finally
        {
            _isLoading = false;
            _refreshButton.Enabled = true;
            UpdateSelectionState();
        }
    }

    private void RestoreSelection(int? maLK)
    {
        _grid.ClearSelection();

        if (_grid.Rows.Count == 0)
        {
            return;
        }

        DataGridViewRow? targetRow = null;
        if (maLK.HasValue)
        {
            targetRow = _grid.Rows
                .Cast<DataGridViewRow>()
                .FirstOrDefault(row => !row.IsNewRow && GetMaLK(row) == maLK.Value);
        }

        targetRow ??= _grid.Rows.Cast<DataGridViewRow>().FirstOrDefault(row => !row.IsNewRow);
        if (targetRow is null)
        {
            return;
        }

        targetRow.Selected = true;
        DataGridViewCell? firstVisibleCell = targetRow.Cells
            .Cast<DataGridViewCell>()
            .FirstOrDefault(cell => cell.Visible);

        if (firstVisibleCell is not null)
        {
            _grid.CurrentCell = firstVisibleCell;
        }
    }

    private void StartSelectedExam()
    {
        int? maLK = GetSelectedMaLK();
        if (!maLK.HasValue)
        {
            MessageBox.Show(
                "Vui lòng chọn một lượt khám trong hàng đợi.",
                "Bắt đầu khám",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        if (_currentUser?.MaNV is not > 0)
        {
            MessageBox.Show(
                "Không xác định được bác sĩ đang đăng nhập. Vui lòng đăng nhập lại trước khi bắt đầu khám.",
                "Bắt đầu khám",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        DialogResult confirm = MessageBox.Show(
            "Chuyển lượt khám đã chọn sang trạng thái Đang khám?",
            "Bắt đầu khám",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button1);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        _startExamButton.Enabled = false;

        try
        {
            bool success = _khamBLL.ChuyenSangDangKham(maLK.Value, _currentUser.MaNV);
            if (!success)
            {
                MessageBox.Show(
                    "Lượt khám này không còn ở trạng thái đang chờ. Hàng đợi sẽ được tải lại.",
                    "Bắt đầu khám",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                RefreshQueue(maLK);
                return;
            }

            ExamStarted?.Invoke(this, new ExamStartedEventArgs(maLK.Value));
            MessageBox.Show(
                "Đã chuyển lượt khám sang trạng thái Đang khám.",
                "Bắt đầu khám",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            RefreshQueue();
        }
        catch
        {
            MessageBox.Show(
                "Không thể bắt đầu khám. Vui lòng thử lại sau.",
                "Bắt đầu khám",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            RefreshQueue(maLK);
        }
    }

    private void UpdateSelectionState()
    {
        int? maLK = GetSelectedMaLK();
        bool hasSelection = maLK.HasValue;

        _startExamButton.Enabled = !_isLoading && hasSelection;
        _selectedLabel.Text = hasSelection
            ? $"Đang chọn lượt khám #{maLK.GetValueOrDefault()}."
            : "Chọn một dòng trong hàng đợi để bắt đầu khám.";
    }

    private int? GetSelectedMaLK()
    {
        return _grid.CurrentRow is null || _grid.CurrentRow.IsNewRow
            ? null
            : GetMaLK(_grid.CurrentRow);
    }

    private int? GetMaLK(DataGridViewRow row)
    {
        object? value = null;

        if (row.DataBoundItem is DataRowView rowView && rowView.Row.Table.Columns.Contains("MaLK"))
        {
            value = rowView["MaLK"];
        }
        else if (_grid.Columns.Contains("MaLK") && row.Cells["MaLK"] is DataGridViewCell maLKCell)
        {
            value = maLKCell.Value;
        }

        if (value is null || value == DBNull.Value)
        {
            return null;
        }

        return int.TryParse(Convert.ToString(value), out int maLK) ? maLK : null;
    }

    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.Value is null)
        {
            return;
        }

        DataGridViewColumn? column = _grid.Columns[e.ColumnIndex];
        if (column?.Name == "TrangThai")
        {
            e.Value = DisplayStatus(Convert.ToString(e.Value));
            e.FormattingApplied = true;
        }
    }

    private static string DisplayStatus(string? status)
    {
        return status switch
        {
            "DangCho" => "Đang chờ",
            "DangKham" => "Đang khám",
            "DaKham" => "Đã khám",
            "DaHuy" => "Đã hủy",
            _ => status ?? string.Empty
        };
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
