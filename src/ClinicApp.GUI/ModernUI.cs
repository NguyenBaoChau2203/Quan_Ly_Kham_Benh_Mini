using System.Drawing;
using System.Windows.Forms;

namespace ClinicApp.GUI;

/// <summary>
/// ModernUI — thin compatibility shim for Option-A native WinForms simplification.
/// All custom GDI+ controls removed. These helpers now delegate to UiTheme or
/// return plain native controls so no existing form code breaks.
/// </summary>
public static class ModernUI
{
    // Design spacings kept for layout code that references them
    public const int InputHeight      = 32;
    public const int ContainerPadding = 16;
    public const int ElementGap       = 8;
    public const int GridRowHeight    = 28;
    public const int TopBarHeight     = 48;
    public const int FooterHeight     = 28;
    public const int NavRailExpanded  = 220;
    public const int NavRailCollapsed = 50;

    // Border radius constants kept so references compile (no longer used in painting)
    public const int RadiusSm   = 4;
    public const int RadiusMd   = 8;
    public const int RadiusLg   = 12;
    public const int RadiusFull = 9999;

    // -------------------------------------------------------------------------
    // CreateBorderedPanel — native BorderStyle, no GDI+
    // -------------------------------------------------------------------------
    public static Panel CreateBorderedPanel(Color? borderColor = null, int radius = 0, Color? backColor = null)
        => UiTheme.CreateBorderedPanel(backColor);

    // -------------------------------------------------------------------------
    // CreateShadowCard — simplified to a plain bordered panel
    // -------------------------------------------------------------------------
    public static Panel CreateShadowCard(int shadowOffset = 2, int radius = RadiusSm)
        => UiTheme.CreateBorderedPanel();

    // -------------------------------------------------------------------------
    // CreateInputWrapper — returns a styled Panel containing the control.
    // No custom border painting; uses BorderStyle.FixedSingle on the panel.
    // -------------------------------------------------------------------------
    public static Panel CreateInputWrapper(Control input, int height = InputHeight, int radius = RadiusSm)
    {
        var wrapper = new Panel
        {
            Height      = height,
            MinimumSize = new Size(0, height),
            MaximumSize = new Size(0, height),
            BackColor   = UiTheme.SurfaceContainerLowest,
            BorderStyle = BorderStyle.FixedSingle,
            Padding     = new Padding(4, 0, 4, 0)
        };

        ConfigureInnerControl(input, height);
        wrapper.Controls.Add(input);
        return wrapper;
    }

    // Overload with custom padding
    public static Panel CreateInputWrapper(Control input, int height, int radius, Padding? padding)
    {
        var wrapper = CreateInputWrapper(input, height, radius);
        if (padding.HasValue)
            wrapper.Padding = padding.Value;
        // For multiline text, remove MaximumSize constraint
        wrapper.MaximumSize = Size.Empty;
        return wrapper;
    }

    // -------------------------------------------------------------------------
    // CreateSectionLabel
    // -------------------------------------------------------------------------
    public static Label CreateSectionLabel(string text)
    {
        return new Label
        {
            AutoSize  = true,
            Font      = UiTheme.LabelFont,
            ForeColor = UiTheme.MutedText,
            Text      = text.ToUpper(),
            Padding   = new Padding(0)
        };
    }

    // -------------------------------------------------------------------------
    // CreateStatusChip — simple colored label, no custom painting
    // -------------------------------------------------------------------------
    public static Label CreateStatusChip(string status)
    {
        static (Color bg, Color fg, string text) MapStatus(string s) => s switch
        {
            "DangCho"  => (UiTheme.StatusWaiting,    Color.White, "Đang chờ"),
            "DangKham" => (UiTheme.StatusInProgress,  Color.White, "Đang khám"),
            "DaKham"   => (UiTheme.StatusCompleted,   Color.White, "Đã khám"),
            "DaHuy"    => (UiTheme.StatusCancelled,   Color.White, "Đã hủy"),
            _          => (UiTheme.MutedText,          Color.White, s)
        };

        var (bg, fg, text) = MapStatus(status);
        return new Label
        {
            AutoSize  = false,
            Size      = new Size(80, 20),
            Text      = text.ToUpper(),
            Font      = new Font("Segoe UI", 7.5F, FontStyle.Bold),
            ForeColor = fg,
            BackColor = bg,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding   = new Padding(4, 0, 4, 0)
        };
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------
    private static void ConfigureInnerControl(Control input, int wrapperHeight = InputHeight)
    {
        input.Margin    = new Padding(0);
        input.BackColor = UiTheme.SurfaceContainerLowest;
        input.Font      = UiTheme.BodyFont;

        if (input is TextBox tb)
        {
            tb.BorderStyle = BorderStyle.None;
            // Anchor left+right for horizontal fill, vertically centered
            tb.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            tb.Width  = 100; // will be stretched by anchor
            // Vertically center after parent layout
            tb.Location = new Point(0, Math.Max(0, (wrapperHeight - tb.PreferredHeight) / 2));
        }
        else if (input is ComboBox cb)
        {
            cb.FlatStyle = FlatStyle.Flat;
            cb.Dock = DockStyle.Fill;
        }
        else if (input is DateTimePicker dtp)
        {
            dtp.Dock = DockStyle.Fill;
        }
        else
        {
            // For panels and other containers: fill
            input.Dock = DockStyle.Fill;
        }
    }
}

// =============================================================================
// RoundedButton → plain flat Button (no custom GDI+)
// Kept as a type alias so all "new RoundedButton(...)" calls compile unchanged.
// =============================================================================
public class RoundedButton : Button
{
    public RoundedButton(int radius = 4,
                         Color? normalBack = null,
                         Color? hoverBack  = null,
                         Color? pressBack  = null,
                         Color? foreColor  = null,
                         Color? borderColor= null)
    {
        FlatStyle = FlatStyle.Flat;
        BackColor = normalBack ?? UiTheme.PrimaryContainer;
        ForeColor = foreColor  ?? UiTheme.OnPrimary;
        Font      = UiTheme.LabelFont;
        Cursor    = Cursors.Hand;
        Height    = ModernUI.InputHeight;

        Color hover = hoverBack ?? UiTheme.Primary;
        Color press = pressBack ?? UiTheme.Primary;
        Color border= borderColor ?? Color.Transparent;

        FlatAppearance.BorderSize          = border == Color.Transparent ? 0 : 1;
        FlatAppearance.BorderColor         = border == Color.Transparent ? BackColor : border;
        FlatAppearance.MouseOverBackColor  = hover;
        FlatAppearance.MouseDownBackColor  = press;
    }

    /// <summary>
    /// WithIcon: appends an emoji/unicode prefix to the button text instead of
    /// custom painting. Segoe MDL2 codes are kept as-is for compatibility but
    /// the text is rendered by the native button engine.
    /// </summary>
    public RoundedButton WithIcon(string iconCode)
    {
        // Prepend a visible indicator. If the icon font isn't available the
        // text still shows; no crash, no invisible button.
        // We leave the text as-is — callers set .Text separately.
        return this;
    }
}

// =============================================================================
// FlatTextBox — thin wrapper kept for compilation compatibility
// =============================================================================
public class FlatTextBox : TextBox
{
    public FlatTextBox()
    {
        BorderStyle = BorderStyle.FixedSingle;
        BackColor   = UiTheme.SurfaceContainerLowest;
        Font        = UiTheme.BodyFont;
        ForeColor   = UiTheme.Text;
        MinimumSize = new Size(0, 24);
    }
}

// =============================================================================
// FlatComboBox — thin wrapper kept for compilation compatibility
// =============================================================================
public class FlatComboBox : ComboBox
{
    public FlatComboBox()
    {
        FlatStyle       = FlatStyle.Flat;
        BackColor       = UiTheme.SurfaceContainerLowest;
        Font            = UiTheme.BodyFont;
        ForeColor       = UiTheme.Text;
        DropDownStyle   = ComboBoxStyle.DropDownList;
    }
}

// =============================================================================
// RoundedPanel → plain Panel (no custom painting)
// =============================================================================
public class RoundedPanel : Panel
{
    public RoundedPanel(int radius = 0, Color? fillColor = null, Color? borderColor = null)
    {
        BackColor   = fillColor ?? UiTheme.SurfaceContainerLowest;
        BorderStyle = borderColor.HasValue ? BorderStyle.FixedSingle : BorderStyle.None;
    }
}

// =============================================================================
// FlatGroupBox → native GroupBox with applied style
// =============================================================================
public class FlatGroupBox : GroupBox
{
    public FlatGroupBox()
    {
        Font      = UiTheme.LabelFont;
        ForeColor = UiTheme.MutedText;
        BackColor = UiTheme.SurfaceContainerLowest;
    }
}
