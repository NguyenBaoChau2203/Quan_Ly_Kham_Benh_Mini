using ClinicApp.BLL;
using ClinicApp.DTO;
using ClinicApp.GUI;

namespace ClinicApp.GUI.Forms;

public class FrmLogin : Form
{
    private readonly AuthBLL _authBLL = new();
    private readonly TextBox _usernameTextBox = new();
    private readonly TextBox _passwordTextBox = new();
    private readonly Button _loginButton = new();
    private readonly Label _messageLabel = new();

    public FrmLogin()
    {
        UiTheme.ApplyForm(this);

        Text = "Đăng nhập hệ thống";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(920, 560);
        Size = new Size(960, 600);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        BuildLayout();

        AcceptButton = _loginButton;
        Shown += (_, _) => _usernameTextBox.Focus();
    }

    private void BuildLayout()
    {
        var shell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Background,
            ColumnCount = 2,
            RowCount = 1
        };
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
        shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        Controls.Add(shell);

        shell.Controls.Add(BuildBrandPanel(), 0, 0);
        shell.Controls.Add(BuildLoginPanel(), 1, 0);
    }

    private static Control BuildBrandPanel()
    {
        var brandPanel = new Panel
        {
            BackColor = UiTheme.Primary,
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 32, 28, 32)
        };

        var footer = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            Font = UiTheme.SmallFont,
            ForeColor = Color.White,
            Text = "WinForms + SQL Server\nGUI → BLL → DAL",
            TextAlign = ContentAlignment.BottomLeft
        };

        var subtitle = new Label
        {
            Dock = DockStyle.Top,
            Height = 72,
            Font = UiTheme.BodyFont,
            ForeColor = Color.White,
            Text = "Quản lý khám bệnh mini với giao diện gọn, rõ và đúng vai trò.",
            TextAlign = ContentAlignment.TopLeft
        };

        var title = new Label
        {
            Dock = DockStyle.Top,
            Height = 72,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = Color.White,
            Text = "Mini Clinic",
            TextAlign = ContentAlignment.BottomLeft
        };

        var badge = new Label
        {
            Dock = DockStyle.Top,
            Height = 28,
            Font = UiTheme.LabelFont,
            ForeColor = Color.White,
            Text = "CLINICAL PRECISION",
            TextAlign = ContentAlignment.MiddleLeft
        };

        brandPanel.Controls.Add(footer);
        brandPanel.Controls.Add(subtitle);
        brandPanel.Controls.Add(title);
        brandPanel.Controls.Add(badge);

        return brandPanel;
    }

    private Control BuildLoginPanel()
    {
        var center = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.Background,
            ColumnCount = 3,
            RowCount = 3
        };
        center.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        center.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420F));
        center.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        center.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        center.RowStyles.Add(new RowStyle(SizeType.Absolute, 360F));
        center.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        var card = new Panel
        {
            BackColor = UiTheme.Surface,
            BorderStyle = BorderStyle.FixedSingle,
            Dock = DockStyle.Fill,
            Padding = new Padding(26, 24, 26, 24)
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 9
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        card.Controls.Add(layout);

        var title = UiTheme.CreateLabel("Đăng nhập", UiTheme.ScreenHeaderFont);
        var subtitle = UiTheme.CreateLabel("Sử dụng tài khoản demo hoặc tài khoản đã được cấp quyền.", UiTheme.BodyFont, UiTheme.MutedText);
        var usernameLabel = UiTheme.CreateLabel("TÊN ĐĂNG NHẬP", UiTheme.LabelFont, UiTheme.MutedText);
        var passwordLabel = UiTheme.CreateLabel("MẬT KHẨU", UiTheme.LabelFont, UiTheme.MutedText);

        ConfigureInput(_usernameTextBox, "tiepnhan");
        ConfigureInput(_passwordTextBox, "123");
        _passwordTextBox.UseSystemPasswordChar = true;

        _messageLabel.Dock = DockStyle.Fill;
        _messageLabel.Font = UiTheme.SmallFont;
        _messageLabel.ForeColor = UiTheme.Error;
        _messageLabel.TextAlign = ContentAlignment.MiddleLeft;

        _loginButton.Text = "Đăng nhập";
        _loginButton.Dock = DockStyle.Fill;
        _loginButton.Click += LoginButton_Click;
        UiTheme.ApplyPrimaryButton(_loginButton);

        layout.Controls.Add(title, 0, 0);
        layout.Controls.Add(subtitle, 0, 1);
        layout.Controls.Add(usernameLabel, 0, 2);
        layout.Controls.Add(_usernameTextBox, 0, 3);
        layout.Controls.Add(passwordLabel, 0, 4);
        layout.Controls.Add(_passwordTextBox, 0, 5);
        layout.Controls.Add(_messageLabel, 0, 6);
        layout.Controls.Add(_loginButton, 0, 7);

        center.Controls.Add(card, 1, 1);
        return center;
    }

    private static void ConfigureInput(TextBox textBox, string placeholderText)
    {
        UiTheme.ApplyTextBox(textBox);
        textBox.Dock = DockStyle.Fill;
        textBox.PlaceholderText = placeholderText;
    }

    private void LoginButton_Click(object? sender, EventArgs e)
    {
        ClearMessage();

        if (string.IsNullOrWhiteSpace(_usernameTextBox.Text))
        {
            ShowError("Vui lòng nhập tên đăng nhập.");
            _usernameTextBox.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(_passwordTextBox.Text))
        {
            ShowError("Vui lòng nhập mật khẩu.");
            _passwordTextBox.Focus();
            return;
        }

        _loginButton.Enabled = false;
        UseWaitCursor = true;

        try
        {
            NhanVienDTO? user = _authBLL.DangNhap(_usernameTextBox.Text, _passwordTextBox.Text);
            if (user is null)
            {
                ShowError("Không đăng nhập được. Vui lòng kiểm tra tài khoản, mật khẩu hoặc kết nối dữ liệu.");
                _passwordTextBox.SelectAll();
                _passwordTextBox.Focus();
                return;
            }

            if (!IsSupportedRole(user.Role))
            {
                ShowError("Tài khoản chưa được phân quyền dùng phiên bản demo.");
                return;
            }

            OpenMainShell(user);
        }
        finally
        {
            UseWaitCursor = false;
            _loginButton.Enabled = true;
        }
    }

    private void OpenMainShell(NhanVienDTO user)
    {
        Hide();

        using var mainForm = new FrmMain(user);
        mainForm.ShowDialog();

        _passwordTextBox.Clear();
        ClearMessage();
        Show();
        Activate();
        _usernameTextBox.Focus();
    }

    private static bool IsSupportedRole(string role)
    {
        return string.Equals(role, "TiepNhan", StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, "BacSi", StringComparison.OrdinalIgnoreCase);
    }

    private void ShowError(string message)
    {
        _messageLabel.Text = message;
    }

    private void ClearMessage()
    {
        _messageLabel.Text = string.Empty;
    }
}
