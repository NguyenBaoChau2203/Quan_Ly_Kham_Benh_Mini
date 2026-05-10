using System.ComponentModel;
using System.Drawing.Drawing2D;
using ClinicApp.BLL;
using ClinicApp.DTO;

namespace ClinicApp.GUI.Forms;

public class FrmLogin : Form
{
    private readonly AuthBLL _authBLL = new();
    private readonly TextBox _txtUsername = CreateInput("Nhập tên đăng nhập");
    private readonly TextBox _txtPassword = CreateInput("Nhập mật khẩu");
    private readonly Label _lblMessage = new()
    {
        Dock = DockStyle.Top,
        Height = 24,
        ForeColor = Color.FromArgb(193, 45, 45),
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        TextAlign = ContentAlignment.MiddleLeft
    };
    private readonly CheckBox _remember = new()
    {
        Dock = DockStyle.Top,
        Height = 28,
        Text = "Ghi nhớ đăng nhập",
        Font = new Font("Segoe UI", 9F),
        ForeColor = Color.FromArgb(82, 97, 122)
    };

    private LoginBackgroundPanel? _background;

    public FrmLogin()
    {
        Text = "Đăng nhập hệ thống - Mini Clinic";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1160, 720);
        Size = new Size(1366, 768);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        DoubleBuffered = true;

        _txtUsername.Text = "bacsi";
        _txtPassword.Text = "123";
        _txtPassword.UseSystemPasswordChar = true;

        BuildLayout();
        AcceptButton = Controls.Find("btnLogin", true).OfType<Button>().FirstOrDefault();
        CancelButton = Controls.Find("btnExit", true).OfType<Button>().FirstOrDefault();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _background?.DisposeBackground();
        }

        base.Dispose(disposing);
    }

    private void BuildLayout()
    {
        _background = new LoginBackgroundPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(62, 56, 62, 56)
        };
        Controls.Add(_background);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ColumnCount = 2,
            RowCount = 1
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
        _background.Controls.Add(root);

        root.Controls.Add(BuildBrandPanel(), 0, 0);
        root.Controls.Add(BuildLoginHost(), 1, 0);
    }

    private Control BuildBrandPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 90, 20, 70)
        };

        var spacer = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = Color.Transparent };
        panel.Controls.Add(spacer);

        panel.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 56,
            Text = "Chăm sóc sức khỏe - Nâng tầm cuộc sống",
            Font = new Font("Segoe UI", 15F, FontStyle.Regular),
            ForeColor = Color.FromArgb(79, 129, 196),
            TextAlign = ContentAlignment.MiddleLeft
        });

        panel.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 60,
            Text = "Quản lý khám bệnh mini",
            Font = new Font("Segoe UI", 22F, FontStyle.Regular),
            ForeColor = Color.FromArgb(76, 132, 202),
            TextAlign = ContentAlignment.MiddleLeft
        });

        panel.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 84,
            Text = "Mini Clinic",
            Font = new Font("Segoe UI", 40F, FontStyle.Bold),
            ForeColor = Color.FromArgb(69, 128, 202),
            TextAlign = ContentAlignment.MiddleLeft
        });

        panel.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 130,
            Text = "+",
            Font = new Font("Segoe UI", 64F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(70, 42, 139, 223),
            TextAlign = ContentAlignment.MiddleCenter
        });

        return panel;
    }

    private Control BuildLoginHost()
    {
        var host = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            ColumnCount = 3,
            RowCount = 3
        };
        host.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        host.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 430));
        host.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        host.RowStyles.Add(new RowStyle(SizeType.Absolute, 540));
        host.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        var card = new RoundedCardPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(28, 26, 28, 24),
            BackColor = Color.Transparent
        };
        host.Controls.Add(card, 1, 1);

        var stack = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        card.Controls.Add(stack);

        stack.Controls.Add(_remember);
        stack.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Color.Transparent });
        stack.Controls.Add(BuildButton("Thoát", false, "btnExit", () => Application.Exit()));
        stack.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Color.Transparent });
        stack.Controls.Add(BuildButton("Đăng nhập", true, "btnLogin", Login));
        stack.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Color.Transparent });
        stack.Controls.Add(_lblMessage);
        stack.Controls.Add(BuildInputField("Mật khẩu", "🔒", _txtPassword, showPasswordToggle: true));
        stack.Controls.Add(BuildInputField("Tên đăng nhập", "👤", _txtUsername, showPasswordToggle: false));

        stack.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 54,
            Text = "Sử dụng tài khoản được cấp để vào hệ thống quản lý khám bệnh.",
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Color.FromArgb(82, 97, 122),
            TextAlign = ContentAlignment.MiddleCenter
        });

        stack.Controls.Add(new AccentLine { Dock = DockStyle.Top, Height = 18 });

        stack.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 42,
            Text = "Đăng nhập hệ thống",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 65, 115),
            TextAlign = ContentAlignment.MiddleCenter
        });

        stack.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 72,
            Text = "👤",
            Font = new Font("Segoe UI Emoji", 36F),
            ForeColor = Color.FromArgb(30, 111, 202),
            TextAlign = ContentAlignment.MiddleCenter
        });

        return host;
    }

    private static TextBox CreateInput(string placeholder)
    {
        return new TextBox
        {
            BorderStyle = BorderStyle.None,
            PlaceholderText = placeholder,
            Font = new Font("Segoe UI", 11F),
            ForeColor = Color.FromArgb(36, 51, 76),
            BackColor = Color.White
        };
    }

    private Control BuildInputField(string label, string icon, TextBox input, bool showPasswordToggle)
    {
        var field = new Panel
        {
            Dock = DockStyle.Top,
            Height = 76,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 0, 0, 10)
        };

        field.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 22,
            Text = label,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(35, 51, 77),
            TextAlign = ContentAlignment.MiddleLeft
        });

        var inputBox = new RoundedInputPanel
        {
            Dock = DockStyle.Top,
            Height = 42,
            Padding = new Padding(12, 9, 12, 8)
        };
        field.Controls.Add(inputBox);
        inputBox.BringToFront();

        inputBox.Controls.Add(new Label
        {
            Dock = DockStyle.Left,
            Width = 34,
            Text = icon,
            Font = new Font("Segoe UI Emoji", 13F),
            ForeColor = Color.FromArgb(38, 62, 98),
            TextAlign = ContentAlignment.MiddleLeft
        });

        if (showPasswordToggle)
        {
            var toggle = new Button
            {
                Dock = DockStyle.Right,
                Width = 34,
                FlatStyle = FlatStyle.Flat,
                Text = "👁",
                Font = new Font("Segoe UI Emoji", 10F),
                ForeColor = Color.FromArgb(96, 111, 136),
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                TabStop = false
            };
            toggle.FlatAppearance.BorderSize = 0;
            toggle.Click += (_, _) => input.UseSystemPasswordChar = !input.UseSystemPasswordChar;
            inputBox.Controls.Add(toggle);
        }

        input.Dock = DockStyle.Fill;
        input.Margin = new Padding(0);
        inputBox.Controls.Add(input);
        input.BringToFront();

        return field;
    }

    private static Control BuildButton(string text, bool primary, string name, Action onClick)
    {
        var button = new RoundedActionButton
        {
            Dock = DockStyle.Top,
            Height = 44,
            Name = name,
            Text = text,
            IsPrimary = primary,
            Cursor = Cursors.Hand
        };
        button.Click += (_, _) => onClick();
        return button;
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

        if (!_remember.Checked)
        {
            _txtPassword.Clear();
        }

        Show();
        Activate();
        _txtUsername.Focus();
    }

    private sealed class LoginBackgroundPanel : Panel
    {
        private readonly Image? _image;

        public LoginBackgroundPanel()
        {
            DoubleBuffered = true;
            string imagePath = Path.Combine(AppContext.BaseDirectory, "Assets", "LoginBackground.png");
            if (File.Exists(imagePath))
            {
                using var stream = File.OpenRead(imagePath);
                _image = Image.FromStream(stream);
            }
        }

        public void DisposeBackground()
        {
            _image?.Dispose();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (_image != null)
            {
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.DrawImage(_image, ClientRectangle);
                using var overlay = new SolidBrush(Color.FromArgb(28, 255, 255, 255));
                e.Graphics.FillRectangle(overlay, ClientRectangle);
                return;
            }

            using var brush = new LinearGradientBrush(ClientRectangle, Color.FromArgb(222, 239, 255), Color.FromArgb(244, 248, 253), 0F);
            e.Graphics.FillRectangle(brush, ClientRectangle);
        }
    }

    private sealed class RoundedCardPanel : Panel
    {
        public RoundedCardPanel()
        {
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new(1, 1, Width - 3, Height - 3);
            using GraphicsPath path = RoundedRect(rect, 8);
            using var shadow = new SolidBrush(Color.FromArgb(36, 55, 75, 100));
            using var fill = new SolidBrush(Color.FromArgb(246, 255, 255, 255));
            using var pen = new Pen(Color.FromArgb(185, 202, 214, 228));
            e.Graphics.FillPath(shadow, Offset(path, 0, 2));
            e.Graphics.FillPath(fill, path);
            e.Graphics.DrawPath(pen, path);
        }
    }

    private sealed class RoundedInputPanel : Panel
    {
        public RoundedInputPanel()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new(0, 0, Width - 1, Height - 1);
            using GraphicsPath path = RoundedRect(rect, 5);
            using var fill = new SolidBrush(Color.White);
            using var pen = new Pen(Color.FromArgb(205, 214, 225));
            e.Graphics.FillPath(fill, path);
            e.Graphics.DrawPath(pen, path);
        }
    }

    private sealed class RoundedActionButton : Button
    {
        private bool _hovered;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsPrimary { get; set; }

        public RoundedActionButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            TextAlign = ContentAlignment.MiddleCenter;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _hovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new(0, 0, Width - 1, Height - 1);
            using GraphicsPath path = RoundedRect(rect, 5);
            Color fill = IsPrimary
                ? (_hovered ? Color.FromArgb(20, 101, 190) : Color.FromArgb(18, 96, 181))
                : (_hovered ? Color.FromArgb(239, 247, 255) : Color.White);
            Color border = Color.FromArgb(18, 96, 181);
            ForeColor = IsPrimary ? Color.White : Color.FromArgb(31, 65, 115);
            using var brush = new SolidBrush(fill);
            using var pen = new Pen(border);
            pevent.Graphics.FillPath(brush, path);
            pevent.Graphics.DrawPath(pen, path);
            TextRenderer.DrawText(pevent.Graphics, Text, Font, rect, ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    private sealed class AccentLine : Control
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int lineWidth = 54;
            int x = (Width - lineWidth) / 2;
            using var pen = new Pen(Color.FromArgb(33, 105, 190), 3F);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawLine(pen, x, Height / 2, x + lineWidth, Height / 2);
        }
    }

    private static GraphicsPath RoundedRect(Rectangle rect, int radius)
    {
        int d = radius * 2;
        var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    private static GraphicsPath Offset(GraphicsPath path, int dx, int dy)
    {
        var clone = (GraphicsPath)path.Clone();
        using var matrix = new Matrix();
        matrix.Translate(dx, dy);
        clone.Transform(matrix);
        return clone;
    }
}
