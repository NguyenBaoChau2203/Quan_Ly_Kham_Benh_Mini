using ClinicApp.GUI;

namespace ClinicApp.GUI.Forms;

public class PlaceholderForm : Form
{
    protected Panel BodyPanel { get; }

    protected PlaceholderForm(string title)
    {
        UiTheme.ApplyForm(this);

        Text = title;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);
        Size = new Size(1040, 680);
        Padding = new Padding(16);

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            BackColor = UiTheme.Background
        };

        var label = new Label
        {
            Dock = DockStyle.Fill,
            Font = UiTheme.ScreenHeaderFont,
            ForeColor = UiTheme.Text,
            Text = title,
            TextAlign = ContentAlignment.MiddleLeft
        };

        BodyPanel = new Panel
        {
            BackColor = UiTheme.Surface,
            BorderStyle = BorderStyle.FixedSingle,
            Dock = DockStyle.Fill,
            Padding = new Padding(16)
        };

        var emptyState = new Label
        {
            Dock = DockStyle.Top,
            Height = 32,
            Font = UiTheme.BodyFont,
            ForeColor = UiTheme.MutedText,
            Text = "Chức năng này chưa triển khai trong phiên bản demo.",
            TextAlign = ContentAlignment.MiddleLeft
        };

        header.Controls.Add(label);
        BodyPanel.Controls.Add(emptyState);
        Controls.Add(BodyPanel);
        Controls.Add(header);
    }
}
