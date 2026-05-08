using System.Data;
using System.Globalization;

namespace ClinicApp.GUI;

internal static class NativeUi
{
    public static Panel Page()
    {
        return new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Background,
            Padding = new Padding(16)
        };
    }

    public static Panel Card(DockStyle dock = DockStyle.None, int height = 0, int width = 0)
    {
        var panel = new Panel
        {
            Dock = dock,
            BackColor = UiTheme.SurfaceContainerLowest,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(12)
        };

        if (height > 0) panel.Height = height;
        if (width > 0) panel.Width = width;
        return panel;
    }

    public static Panel Bar(DockStyle dock, int height, Color? backColor = null)
    {
        return new Panel
        {
            Dock = dock,
            Height = height,
            BackColor = backColor ?? UiTheme.Surface,
            Padding = new Padding(16, 0, 16, 0)
        };
    }

    public static Label Title(string text)
    {
        return new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 30,
            Text = text,
            Font = UiTheme.ScreenHeaderFont,
            ForeColor = UiTheme.Primary,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    public static Label Section(string text)
    {
        return new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 26,
            Text = text,
            Font = UiTheme.SectionHeaderFont,
            ForeColor = UiTheme.Primary,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    public static Label FieldLabel(string text)
    {
        return new Label
        {
            AutoSize = false,
            Height = 18,
            Dock = DockStyle.Top,
            Text = text,
            Font = UiTheme.LabelFont,
            ForeColor = UiTheme.MutedText,
            TextAlign = ContentAlignment.BottomLeft
        };
    }

    public static TextBox TextBox(string placeholder = "")
    {
        var box = new TextBox { PlaceholderText = placeholder };
        UiTheme.ApplyTextBox(box);
        return box;
    }

    public static TextBox MultilineTextBox(string placeholder = "", int height = 88)
    {
        var box = TextBox(placeholder);
        box.Multiline = true;
        box.Height = height;
        box.ScrollBars = ScrollBars.Vertical;
        return box;
    }

    public static Button PrimaryButton(string text)
    {
        var button = new Button { Text = text };
        UiTheme.ApplyPrimaryButton(button);
        return button;
    }

    public static Button SecondaryButton(string text)
    {
        var button = new Button { Text = text };
        UiTheme.ApplySecondaryButton(button);
        return button;
    }

    public static Button DangerButton(string text)
    {
        var button = new Button { Text = text };
        UiTheme.ApplyDangerButton(button);
        return button;
    }

    public static DataGridView Grid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoGenerateColumns = true
        };

        UiTheme.ApplyGrid(grid);
        grid.CellFormatting += FormatCommonGridCell;
        return grid;
    }

    private static void FormatCommonGridCell(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (sender is not DataGridView grid || e.RowIndex < 0 || e.ColumnIndex < 0) return;

        string columnName = grid.Columns[e.ColumnIndex].Name;
        if (columnName.Contains("Ngay", StringComparison.OrdinalIgnoreCase) && e.Value is DateTime date)
        {
            e.Value = columnName.Contains("Kham", StringComparison.OrdinalIgnoreCase)
                ? date.ToString("dd/MM/yyyy HH:mm")
                : date.ToString("dd/MM/yyyy");
            e.FormattingApplied = true;
            return;
        }

        if (columnName.Equals("TrangThai", StringComparison.OrdinalIgnoreCase) && e.Value != null)
        {
            e.Value = StatusText(Convert.ToString(e.Value));
            e.FormattingApplied = true;
        }
    }

    public static FlowLayoutPanel Toolbar(DockStyle dock = DockStyle.Top)
    {
        return new FlowLayoutPanel
        {
            Dock = dock,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = UiTheme.SurfaceContainerLowest,
            Padding = new Padding(0, 4, 0, 4)
        };
    }

    public static TableLayoutPanel FormGrid(int columns = 2)
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = columns,
            RowCount = 1,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };

        for (int i = 0; i < columns; i++)
        {
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
        }

        return table;
    }

    public static void ConfigureSplitter(SplitContainer split, int desiredDistance, int panel1MinSize, int panel2MinSize)
    {
        split.Panel1MinSize = 0;
        split.Panel2MinSize = 0;

        void ApplyDistance()
        {
            if (split.IsDisposed) return;

            int total = split.Orientation == Orientation.Vertical ? split.Width : split.Height;
            int max = total - panel2MinSize - split.SplitterWidth;
            if (max < panel1MinSize)
            {
                return;
            }

            split.Panel1MinSize = panel1MinSize;
            split.Panel2MinSize = panel2MinSize;
            split.SplitterDistance = Math.Clamp(desiredDistance, panel1MinSize, max);
        }

        split.HandleCreated += (_, _) => split.BeginInvoke((MethodInvoker)ApplyDistance);
        split.SizeChanged += (_, _) =>
        {
            if (split.IsHandleCreated)
            {
                ApplyDistance();
            }
        };
    }

    public static Panel Field(string label, Control input)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = input.Height + 24,
            Padding = new Padding(0, 0, 0, 6),
            Margin = new Padding(0, 0, 0, 6)
        };

        input.Dock = DockStyle.Top;
        panel.Controls.Add(input);
        panel.Controls.Add(FieldLabel(label));
        return panel;
    }

    public static string TextOf(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value) return string.Empty;
        return Convert.ToString(row[columnName], CultureInfo.CurrentCulture) ?? string.Empty;
    }

    public static int IntOf(DataRow row, string columnName)
    {
        return int.TryParse(TextOf(row, columnName), out int value) ? value : 0;
    }

    public static DateTime? DateOf(DataRow row, string columnName)
    {
        if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value) return null;
        return Convert.ToDateTime(row[columnName], CultureInfo.CurrentCulture);
    }

    public static string DateText(object? value)
    {
        if (value == null || value == DBNull.Value) return "";
        return Convert.ToDateTime(value, CultureInfo.CurrentCulture).ToString("dd/MM/yyyy");
    }

    public static string DateTimeText(object? value)
    {
        if (value == null || value == DBNull.Value) return "";
        return Convert.ToDateTime(value, CultureInfo.CurrentCulture).ToString("dd/MM/yyyy HH:mm");
    }

    public static string StatusText(string? status)
    {
        return status switch
        {
            "DangCho" => "Đang chờ",
            "DangKham" => "Đang khám",
            "DaKham" => "Đã khám",
            "DaHuy" => "Đã hủy",
            _ => status ?? ""
        };
    }

    public static void ShowError(string message)
    {
        MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public static void ShowInfo(string message)
    {
        MessageBox.Show(message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
