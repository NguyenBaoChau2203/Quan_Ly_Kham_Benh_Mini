namespace ClinicApp.GUI.Forms;

public class PlaceholderForm : Form
{
    protected PlaceholderForm(string title)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);

        var label = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            Text = title,
            Location = new Point(24, 24)
        };

        Controls.Add(label);
    }
}
