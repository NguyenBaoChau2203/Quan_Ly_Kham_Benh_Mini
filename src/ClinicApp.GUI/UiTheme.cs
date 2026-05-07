using System.Drawing;
using System.Windows.Forms;

namespace ClinicApp.GUI;

internal static class UiTheme
{
    public const int NavigationWidth = 220;
    public const int TopBarHeight = 56;

    public static readonly Color Primary = ColorTranslator.FromHtml("#005596");
    public static readonly Color PrimaryDark = ColorTranslator.FromHtml("#003e6f");
    public static readonly Color Background = ColorTranslator.FromHtml("#f8f9ff");
    public static readonly Color Surface = Color.White;
    public static readonly Color SurfaceLow = ColorTranslator.FromHtml("#f2f3f9");
    public static readonly Color Border = ColorTranslator.FromHtml("#c1c7d2");
    public static readonly Color Text = ColorTranslator.FromHtml("#191c20");
    public static readonly Color MutedText = ColorTranslator.FromHtml("#414750");
    public static readonly Color Error = ColorTranslator.FromHtml("#ba1a1a");

    public static readonly Font BodyFont = new("Segoe UI", 9F, FontStyle.Regular);
    public static readonly Font SmallFont = new("Segoe UI", 8.25F, FontStyle.Regular);
    public static readonly Font LabelFont = new("Segoe UI", 8.25F, FontStyle.Bold);
    public static readonly Font SectionHeaderFont = new("Segoe UI", 10.5F, FontStyle.Bold);
    public static readonly Font ScreenHeaderFont = new("Segoe UI", 12F, FontStyle.Bold);
    public static readonly Font AppTitleFont = new("Segoe UI", 14F, FontStyle.Bold);

    public static void ApplyForm(Form form)
    {
        form.Font = BodyFont;
        form.BackColor = Background;
        form.ForeColor = Text;
    }

    public static void ApplyPrimaryButton(Button button)
    {
        button.Font = LabelFont;
        button.Height = 32;
        button.Cursor = Cursors.Hand;
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = Primary;
        button.ForeColor = Color.White;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = PrimaryDark;
        button.FlatAppearance.MouseDownBackColor = PrimaryDark;
    }

    public static void ApplySecondaryButton(Button button)
    {
        button.Font = LabelFont;
        button.Height = 32;
        button.Cursor = Cursors.Hand;
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = Surface;
        button.ForeColor = Primary;
        button.FlatAppearance.BorderColor = Primary;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = SurfaceLow;
        button.FlatAppearance.MouseDownBackColor = SurfaceLow;
    }

    public static void ApplyNavigationButton(Button button, bool selected)
    {
        button.Width = NavigationWidth;
        button.Height = 42;
        button.Font = LabelFont;
        button.TextAlign = ContentAlignment.MiddleLeft;
        button.Padding = new Padding(18, 0, 8, 0);
        button.Cursor = Cursors.Hand;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = selected ? Primary : Surface;
        button.ForeColor = selected ? Color.White : Text;
        button.FlatAppearance.MouseOverBackColor = selected ? PrimaryDark : SurfaceLow;
        button.FlatAppearance.MouseDownBackColor = selected ? PrimaryDark : SurfaceLow;
    }

    public static void ApplyTextBox(TextBox textBox)
    {
        textBox.Font = BodyFont;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.Margin = new Padding(0, 0, 0, 8);
        textBox.MinimumSize = new Size(0, 32);
    }

    public static Label CreateLabel(string text, Font font, Color? color = null)
    {
        return new Label
        {
            AutoSize = false,
            Dock = DockStyle.Top,
            Font = font,
            ForeColor = color ?? Text,
            Text = text,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    public static Panel CreateDivider()
    {
        return new Panel
        {
            BackColor = Border,
            Dock = DockStyle.Top,
            Height = 1
        };
    }
}
