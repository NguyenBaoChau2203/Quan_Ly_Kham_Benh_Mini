using ClinicApp.BLL;
using ClinicApp.DTO;

namespace ClinicApp.GUI.Forms;

public class FrmLogin : Form
{
    private readonly AuthBLL _authBLL = new();
    private readonly TextBox _txtUsername = NativeUi.TextBox("Tên đăng nhập");
    private readonly TextBox _txtPassword = NativeUi.TextBox("Mật khẩu");
    private readonly Label _lblMessage = new()
    {
        Dock = DockStyle.Top,
        Height = 24,
        ForeColor = UiTheme.Error,
        Font = UiTheme.LabelFont,
        TextAlign = ContentAlignment.MiddleLeft
    };

    public FrmLogin()
    {
        UiTheme.ApplyForm(this);
        Text = "Đăng nhập hệ thống - Mini Clinic";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(980, 600);
        Size = new Size(1120, 660);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        BuildLayout();
        AcceptButton = Controls.Find("btnLogin", true).OfType<Button>().FirstOrDefault();
        CancelButton = Controls.Find("btnExit", true).OfType<Button>().FirstOrDefault();
    }

    private void BuildLayout()
    {
        var page = NativeUi.Page();
        page.Padding = new Padding(0);
        Controls.Add(page);

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            BackColor = UiTheme.SurfaceContainerLowest,
            Padding = new Padding(18, 0, 18, 0)
        };
        page.Controls.Add(header);

        header.Controls.Add(new Label
        {
            Dock = DockStyle.Left,
            Width = 420,
            Text = "Mini Clinic Management",
            Font = new Font("Segoe UI", 15F, FontStyle.Bold),
            ForeColor = UiTheme.Primary,
            TextAlign = ContentAlignment.MiddleLeft
        });
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Right,
            Width = 160,
            Text = "Version 2.4.0",
            Font = UiTheme.LabelFont,
            ForeColor = UiTheme.MutedText,
            TextAlign = ContentAlignment.MiddleRight
        });

        var center = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            BackColor = UiTheme.Background
        };
        center.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        center.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 420));
        center.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        center.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        center.RowStyles.Add(new RowStyle(SizeType.Absolute, 360));
        center.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        page.Controls.Add(center);

        var card = NativeUi.Card(DockStyle.Fill);
        card.Padding = new Padding(24);
        center.Controls.Add(card, 1, 1);

        card.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 42,
            Text = "Đăng nhập hệ thống",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = UiTheme.Primary,
            TextAlign = ContentAlignment.MiddleLeft
        });

        _txtUsername.Text = "bacsi";
        _txtPassword.Text = "123";
        _txtPassword.UseSystemPasswordChar = true;

        var btnLogin = NativeUi.PrimaryButton("Đăng nhập");
        btnLogin.Name = "btnLogin";
        btnLogin.Click += (_, _) => Login();

        var btnExit = NativeUi.SecondaryButton("Thoát");
        btnExit.Name = "btnExit";
        btnExit.Click += (_, _) => Application.Exit();

        var buttons = new TableLayoutPanel { Dock = DockStyle.Top, Height = 42, ColumnCount = 2 };
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
        buttons.Controls.Add(btnLogin, 0, 0);
        buttons.Controls.Add(btnExit, 1, 0);
        btnLogin.Dock = DockStyle.Fill;
        btnExit.Dock = DockStyle.Fill;
        btnExit.Margin = new Padding(8, 0, 0, 0);

        var remember = new CheckBox
        {
            Dock = DockStyle.Top,
            Height = 28,
            Text = "Ghi nhớ đăng nhập",
            Font = UiTheme.SmallFont,
            ForeColor = UiTheme.MutedText
        };

        card.Controls.Add(remember);
        card.Controls.Add(buttons);
        card.Controls.Add(_lblMessage);
        card.Controls.Add(NativeUi.Field("Mật khẩu", _txtPassword));
        card.Controls.Add(NativeUi.Field("Tên đăng nhập", _txtUsername));
        card.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 46,
            Text = "Sử dụng tài khoản được cấp để vào hệ thống quản lý khám bệnh.",
            Font = UiTheme.BodyFont,
            ForeColor = UiTheme.MutedText
        });

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            BackColor = UiTheme.SurfaceContainerLow,
            Padding = new Padding(16, 0, 16, 0)
        };
        page.Controls.Add(footer);
        footer.Controls.Add(new Label
        {
            Dock = DockStyle.Left,
            Width = 360,
            Text = "Hệ thống: Sẵn sàng",
            Font = UiTheme.SmallFont,
            ForeColor = UiTheme.MutedText,
            TextAlign = ContentAlignment.MiddleLeft
        });
    }

    private void Login()
    {
        _lblMessage.Text = string.Empty;

        string username = _txtUsername.Text.Trim();
        string password = _txtPassword.Text.Trim();
        if (username.Length == 0 || password.Length == 0)
        {
            _lblMessage.Text = "Vui lòng nhập tài khoản và mật khẩu.";
            return;
        }

        NhanVienDTO? user = _authBLL.DangNhap(username, password);
        if (user == null)
        {
            _lblMessage.Text = "Tài khoản hoặc mật khẩu không đúng.";
            _txtPassword.SelectAll();
            _txtPassword.Focus();
            return;
        }

        if (!string.Equals(user.Role, "TiepNhan", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(user.Role, "BacSi", StringComparison.OrdinalIgnoreCase))
        {
            _lblMessage.Text = "Tài khoản chưa được phân quyền.";
            return;
        }

        OpenMainShell(user);
    }

    private void OpenMainShell(NhanVienDTO user)
    {
        Hide();
        using var mainForm = new FrmMain(user);
        mainForm.ShowDialog(this);

        _txtPassword.Clear();
        Show();
        Activate();
        _txtUsername.Focus();
    }
}
