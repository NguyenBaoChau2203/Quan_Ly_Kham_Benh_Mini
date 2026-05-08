using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ClinicApp.GUI;

/// <summary>
/// Clinical Precision design system — native WinForms edition.
/// No custom GDI+ painting. All styling uses native control properties only.
/// </summary>
internal static class UiTheme
{
    public const int NavigationWidth = 220;
    public const int TopBarHeight = 48;

    // =====================================================
    // COLOR PALETTE — Clinical Precision
    // =====================================================
    public static readonly Color Background            = ColorTranslator.FromHtml("#f8f9ff");
    public static readonly Color Surface               = ColorTranslator.FromHtml("#f8f9ff");
    public static readonly Color SurfaceContainerLowest = ColorTranslator.FromHtml("#ffffff");
    public static readonly Color SurfaceContainerLow   = ColorTranslator.FromHtml("#f2f3f9");
    public static readonly Color SurfaceContainer      = ColorTranslator.FromHtml("#ededf4");
    public static readonly Color SurfaceContainerHigh  = ColorTranslator.FromHtml("#e7e8ee");
    public static readonly Color SurfaceContainerHighest= ColorTranslator.FromHtml("#e1e2e8");

    public static readonly Color Text      = ColorTranslator.FromHtml("#191c20");
    public static readonly Color MutedText = ColorTranslator.FromHtml("#414750");
    public static readonly Color Outline   = ColorTranslator.FromHtml("#727781");
    public static readonly Color Border    = ColorTranslator.FromHtml("#c1c7d2");

    public static readonly Color Primary            = ColorTranslator.FromHtml("#003e6f");
    public static readonly Color OnPrimary          = ColorTranslator.FromHtml("#ffffff");
    public static readonly Color PrimaryContainer   = ColorTranslator.FromHtml("#005596");
    public static readonly Color OnPrimaryContainer = ColorTranslator.FromHtml("#a4caff");

    public static readonly Color Secondary            = ColorTranslator.FromHtml("#505f76");
    public static readonly Color SecondaryContainer   = ColorTranslator.FromHtml("#d0e1fb");
    public static readonly Color OnSecondaryContainer = ColorTranslator.FromHtml("#54647a");

    public static readonly Color Tertiary          = ColorTranslator.FromHtml("#642d00");
    public static readonly Color TertiaryContainer = ColorTranslator.FromHtml("#873f01");

    public static readonly Color Error            = ColorTranslator.FromHtml("#ba1a1a");
    public static readonly Color ErrorContainer   = ColorTranslator.FromHtml("#ffdad6");
    public static readonly Color OnErrorContainer = ColorTranslator.FromHtml("#93000a");

    public static readonly Color GridRowAlt = ColorTranslator.FromHtml("#F0F5FF");

    public static readonly Color StatusWaiting   = PrimaryContainer;
    public static readonly Color StatusInProgress= ColorTranslator.FromHtml("#d97706");
    public static readonly Color StatusCompleted = ColorTranslator.FromHtml("#16a34a");
    public static readonly Color StatusCancelled = ColorTranslator.FromHtml("#dc2626");

    // =====================================================
    // TYPOGRAPHY
    // =====================================================
    public static readonly Font BodyFont         = new("Segoe UI", 9F, FontStyle.Regular);
    public static readonly Font SmallFont        = new("Segoe UI", 8.25F, FontStyle.Regular);
    public static readonly Font LabelFont        = new("Segoe UI", 8.25F, FontStyle.Bold);
    public static readonly Font SectionHeaderFont= new("Segoe UI", 10.5F, FontStyle.Bold);
    public static readonly Font ScreenHeaderFont = new("Segoe UI", 12F, FontStyle.Bold);
    public static readonly Font AppTitleFont     = new("Segoe UI", 12F, FontStyle.Bold);
    // Keep IconFont for FrmInPhieu / DrawStatusChip only
    public static readonly Font IconFont      = new("Segoe MDL2 Assets", 12F, FontStyle.Regular);
    public static readonly Font SmallIconFont = new("Segoe MDL2 Assets", 9F, FontStyle.Regular);

    // =====================================================
    // ICONS (used only in DrawStatusChip + FrmInPhieu)
    // =====================================================
    public static class Icons
    {
        public const string Person    = "\uE77B";
        public const string Heartbeat = "\uE7FB";
        public const string MedicalBag= "\uE814";
        public const string Save      = "\uE74E";
        public const string Back      = "\uE72B";
        public const string Print     = "\uE749";
        public const string Close     = "\uE711";
        public const string Dashboard = "\uE909";
        public const string Add       = "\uE710";
        public const string People    = "\uE8FD";
        public const string History   = "\uE81C";
        public const string Settings  = "\uE713";
        public const string Group     = "\uE716";
        public const string Checkmark = "\uE73E";
        public const string Clock     = "\uE121";
        public const string Search    = "\uE71E";
        public const string Refresh   = "\uE72C";
        public const string Play      = "\uE768";
    }

    // =====================================================
    // FORM STYLE
    // =====================================================
    public static void ApplyForm(Form form)
    {
        form.Font      = BodyFont;
        form.BackColor = Background;
        form.ForeColor = Text;
    }

    // =====================================================
    // BUTTON STYLES — flat native, no custom GDI+
    // =====================================================
    public static void ApplyPrimaryButton(Button button)
    {
        button.Font      = LabelFont;
        button.Height    = 32;
        button.Cursor    = Cursors.Hand;
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = PrimaryContainer;
        button.ForeColor = OnPrimary;
        button.FlatAppearance.BorderSize           = 0;
        button.FlatAppearance.MouseOverBackColor   = Primary;
        button.FlatAppearance.MouseDownBackColor   = Primary;
    }

    public static void ApplySecondaryButton(Button button)
    {
        button.Font      = LabelFont;
        button.Height    = 32;
        button.Cursor    = Cursors.Hand;
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = SurfaceContainerLowest;
        button.ForeColor = PrimaryContainer;
        button.FlatAppearance.BorderColor          = PrimaryContainer;
        button.FlatAppearance.BorderSize           = 1;
        button.FlatAppearance.MouseOverBackColor   = SurfaceContainerLow;
        button.FlatAppearance.MouseDownBackColor   = SurfaceContainerLow;
    }

    public static void ApplyDangerButton(Button button)
    {
        button.Font      = LabelFont;
        button.Height    = 32;
        button.Cursor    = Cursors.Hand;
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = ErrorContainer;
        button.ForeColor = Error;
        button.FlatAppearance.BorderColor          = Error;
        button.FlatAppearance.BorderSize           = 1;
        button.FlatAppearance.MouseOverBackColor   = ColorTranslator.FromHtml("#ffc0bc");
        button.FlatAppearance.MouseDownBackColor   = ColorTranslator.FromHtml("#ffc0bc");
    }

    public static void ApplyNavigationButton(Button button, bool selected)
    {
        button.Width     = NavigationWidth;
        button.Height    = 42;
        button.Font      = selected ? new Font(BodyFont, FontStyle.Bold) : BodyFont;
        button.TextAlign = ContentAlignment.MiddleLeft;
        button.ImageAlign= ContentAlignment.MiddleLeft;
        button.Padding   = new Padding(18, 0, 8, 0);
        button.Cursor    = Cursors.Hand;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;

        if (selected)
        {
            button.BackColor = PrimaryContainer;
            button.ForeColor = OnPrimary;
        }
        else
        {
            button.BackColor = SurfaceContainer;
            button.ForeColor = MutedText;
        }

        button.FlatAppearance.MouseOverBackColor = selected ? PrimaryContainer : SurfaceContainerHigh;
        button.FlatAppearance.MouseDownBackColor = selected ? PrimaryContainer : SurfaceContainerHigh;
    }

    // =====================================================
    // INPUT FIELD STYLES — native BorderStyle, no wrappers
    // =====================================================
    public static void ApplyTextBox(TextBox textBox)
    {
        textBox.Font        = BodyFont;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.BackColor   = SurfaceContainerLowest;
        textBox.ForeColor   = Text;
        textBox.Height      = 32;
        textBox.Margin      = new Padding(0, 0, 0, 4);
    }

    public static void ApplyComboBox(ComboBox comboBox)
    {
        comboBox.Font        = BodyFont;
        comboBox.FlatStyle   = FlatStyle.Flat;
        comboBox.BackColor   = SurfaceContainerLowest;
        comboBox.ForeColor   = Text;
        comboBox.Height      = 32;
    }

    public static void ApplyDateTimePicker(DateTimePicker dtp)
    {
        dtp.Font   = BodyFont;
        dtp.Height = 32;
    }

    // =====================================================
    // GRID STYLE — flat solid header, no 3D highlights
    // =====================================================
    public static void ApplyGrid(DataGridView grid)
    {
        grid.EnableHeadersVisualStyles  = false;
        grid.BackgroundColor            = SurfaceContainerLowest;
        grid.BorderStyle                = BorderStyle.None;
        grid.CellBorderStyle            = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor                  = Border;
        grid.RowHeadersVisible          = false;
        grid.AutoSizeColumnsMode        = DataGridViewAutoSizeColumnsMode.Fill;

        grid.ColumnHeadersBorderStyle              = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersHeight                   = 32;
        grid.ColumnHeadersHeightSizeMode           = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersDefaultCellStyle.BackColor       = PrimaryContainer;
        grid.ColumnHeadersDefaultCellStyle.ForeColor       = OnPrimary;
        grid.ColumnHeadersDefaultCellStyle.Font            = LabelFont;
        grid.ColumnHeadersDefaultCellStyle.Padding         = new Padding(8, 0, 8, 0);
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = PrimaryContainer;
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = OnPrimary;

        grid.DefaultCellStyle.BackColor         = SurfaceContainerLowest;
        grid.DefaultCellStyle.ForeColor         = Text;
        grid.DefaultCellStyle.Font              = BodyFont;
        grid.DefaultCellStyle.Padding           = new Padding(8, 0, 8, 0);
        grid.DefaultCellStyle.SelectionBackColor= SurfaceContainerHigh;
        grid.DefaultCellStyle.SelectionForeColor= Text;

        grid.AlternatingRowsDefaultCellStyle.BackColor = GridRowAlt;
        grid.RowTemplate.Height = 28;
        grid.RowTemplate.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);
    }

    // =====================================================
    // LAYOUT HELPERS — simple panels, no custom paint
    // =====================================================
    public static Label CreateLabel(string text, Font font, Color? color = null)
    {
        return new Label
        {
            AutoSize  = false,
            Dock      = DockStyle.Top,
            Font      = font,
            ForeColor = color ?? Text,
            Text      = text,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    /// <summary>Creates a 1px horizontal rule panel.</summary>
    public static Panel CreateDivider()
    {
        return new Panel
        {
            BackColor = Border,
            Dock      = DockStyle.Top,
            Height    = 1
        };
    }

    /// <summary>
    /// Creates a flat bordered card panel (1px solid border, no GDI+).
    /// Uses native WinForms BorderStyle via a thin outer panel trick.
    /// </summary>
    public static Panel CreateCard(Color? backColor = null)
    {
        return new Panel
        {
            BackColor = backColor ?? SurfaceContainerLowest,
            Dock      = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    /// <summary>
    /// Creates a simple bordered container panel using native BorderStyle.
    /// Drop-in replacement for the old ModernUI.CreateBorderedPanel.
    /// </summary>
    public static Panel CreateBorderedPanel(Color? backColor = null)
    {
        return new Panel
        {
            BackColor   = backColor ?? SurfaceContainerLowest,
            BorderStyle = BorderStyle.FixedSingle
        };
    }

    // =====================================================
    // STATUS CHIP DRAWING (DataGridView CellPainting only)
    // This is the ONE place we keep GDI+ — it is well-contained
    // inside DataGridView cell painting, not a control wrapper.
    // =====================================================
    public static void DrawStatusChip(Graphics g, Rectangle bounds, string status, Font font)
    {
        Color backColor;
        string text;

        switch (status)
        {
            case "DangCho":
                backColor = StatusWaiting;
                text = "Đang chờ";
                break;
            case "DangKham":
                backColor = StatusInProgress;
                text = "Đang khám";
                break;
            case "DaKham":
                backColor = StatusCompleted;
                text = "Đã khám";
                break;
            case "DaHuy":
                backColor = StatusCancelled;
                text = "Đã hủy";
                break;
            default:
                backColor = MutedText;
                text = status ?? "";
                break;
        }

        g.SmoothingMode = SmoothingMode.AntiAlias;
        int chipHeight = 20;
        int chipWidth  = (int)g.MeasureString(text.ToUpper(), font).Width + 16;
        int chipX      = bounds.X + (bounds.Width  - chipWidth)  / 2;
        int chipY      = bounds.Y + (bounds.Height - chipHeight) / 2;
        var chipRect   = new Rectangle(chipX, chipY, chipWidth, chipHeight);

        using var brush = new SolidBrush(backColor);
        g.FillRectangle(brush, chipRect);

        TextRenderer.DrawText(g, text.ToUpper(), font, chipRect, Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }

    // =====================================================
    // GROUP BOX STYLE (native, no custom painting)
    // =====================================================
    public static void ApplyGroupBox(GroupBox gb)
    {
        gb.Font      = LabelFont;
        gb.ForeColor = MutedText;
        gb.BackColor = SurfaceContainerLowest;
    }

    // =====================================================
    // LABEL HELPER
    // =====================================================
    public static Label CreateFieldLabel(string text)
    {
        return new Label
        {
            Text      = text,
            Font      = LabelFont,
            ForeColor = MutedText,
            AutoSize  = false,
            Dock      = DockStyle.Top,
            Height    = 18,
            TextAlign = ContentAlignment.BottomLeft,
            Padding   = new Padding(1, 0, 0, 0)
        };
    }

    /// <summary>
    /// Creates a native GroupBox with title and inner content control.
    /// Native rendering — no custom GDI+ painting.
    /// </summary>
    public static GroupBox CreateFlatGroupBox(string title, Control content)
    {
        var gb = new GroupBox
        {
            Text      = title,
            Dock      = DockStyle.Fill,
            Margin    = new Padding(0, 0, 0, 8),
            Padding   = new Padding(8, 16, 8, 8),
            Font      = LabelFont,
            ForeColor = MutedText,
            BackColor = SurfaceContainerLowest
        };

        content.Dock = DockStyle.Fill;
        gb.Controls.Add(content);

        if (content is TextBox tb) tb.BorderStyle = BorderStyle.FixedSingle;

        return gb;
    }

    /// <summary>
    /// Creates a flat bordered input panel using native BorderStyle.
    /// </summary>
    public static Panel CreateFlatInputPanel(Control content, int height = 32)
    {
        var panel = new Panel
        {
            Height      = height,
            MinimumSize = new Size(0, height),
            BackColor   = SurfaceContainerLowest,
            BorderStyle = BorderStyle.FixedSingle,
            Padding     = new Padding(4, 0, 4, 0)
        };

        content.Dock = DockStyle.Fill;
        content.Margin = new Padding(0);
        if (content is TextBox tb)
        {
            tb.BorderStyle = BorderStyle.None;
            tb.BackColor   = SurfaceContainerLowest;
        }
        else if (content is ComboBox cb)
        {
            cb.FlatStyle = FlatStyle.Flat;
            cb.BackColor = SurfaceContainerLowest;
        }

        panel.Controls.Add(content);
        return panel;
    }
}

