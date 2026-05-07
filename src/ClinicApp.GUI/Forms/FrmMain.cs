using ClinicApp.DTO;
using ClinicApp.GUI;

namespace ClinicApp.GUI.Forms;

public class FrmMain : Form
{
    private readonly NhanVienDTO _currentUser;
    private readonly List<ModuleInfo> _modules;
    private readonly FlowLayoutPanel _navigationList = new();
    private readonly Panel _contentPanel = new();
    private readonly Dictionary<string, Button> _navigationButtons = new(StringComparer.OrdinalIgnoreCase);
    private Button? _selectedNavigationButton;

    public FrmMain(NhanVienDTO currentUser)
    {
        _currentUser = currentUser;
        _modules = BuildModules(currentUser.Role);

        UiTheme.ApplyForm(this);

        Text = "Mini Clinic Management";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1100, 680);
        Size = new Size(1220, 740);
        WindowState = FormWindowState.Maximized;

        BuildShell();
        Load += (_, _) => SelectInitialModule();
    }

    private void BuildShell()
    {
        Controls.Add(BuildContentPanel());
        Controls.Add(BuildNavigationPanel());
        Controls.Add(BuildTopBar());
    }

    private Control BuildTopBar()
    {
        var topBar = new Panel
        {
            BackColor = UiTheme.Primary,
            Dock = DockStyle.Top,
            Height = UiTheme.TopBarHeight,
            Padding = new Padding(16, 10, 16, 10)
        };

        var title = new Label
        {
            Dock = DockStyle.Left,
            Width = 420,
            Font = UiTheme.AppTitleFont,
            ForeColor = Color.White,
            Text = "Mini Clinic Management",
            TextAlign = ContentAlignment.MiddleLeft
        };

        var logoutButton = new Button
        {
            Dock = DockStyle.Right,
            Width = 104,
            Text = "Đăng xuất"
        };
        UiTheme.ApplySecondaryButton(logoutButton);
        logoutButton.Click += LogoutButton_Click;

        var userLabel = new Label
        {
            Dock = DockStyle.Right,
            Width = 360,
            Font = UiTheme.BodyFont,
            ForeColor = Color.White,
            Text = $"{DisplayName(_currentUser)}  •  {RoleDisplayName(_currentUser.Role)}",
            TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 0, 14, 0)
        };

        topBar.Controls.Add(title);
        topBar.Controls.Add(logoutButton);
        topBar.Controls.Add(userLabel);

        return topBar;
    }

    private Control BuildNavigationPanel()
    {
        var navigationPanel = new Panel
        {
            BackColor = UiTheme.Surface,
            Dock = DockStyle.Left,
            Width = UiTheme.NavigationWidth,
            Padding = new Padding(0, 0, 0, 8)
        };

        var rolePanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 76,
            Padding = new Padding(18, 12, 12, 8),
            BackColor = UiTheme.Surface
        };

        var roleLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 26,
            Font = UiTheme.SectionHeaderFont,
            ForeColor = UiTheme.Text,
            Text = RoleDisplayName(_currentUser.Role),
            TextAlign = ContentAlignment.BottomLeft
        };

        var usernameLabel = new Label
        {
            Dock = DockStyle.Top,
            Height = 22,
            Font = UiTheme.SmallFont,
            ForeColor = UiTheme.MutedText,
            Text = _currentUser.Username,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _navigationList.Dock = DockStyle.Fill;
        _navigationList.FlowDirection = FlowDirection.TopDown;
        _navigationList.WrapContents = false;
        _navigationList.AutoScroll = true;
        _navigationList.BackColor = UiTheme.Surface;

        foreach (ModuleInfo module in _modules)
        {
            var button = new Button
            {
                Text = module.Title,
                Tag = module.Key
            };
            UiTheme.ApplyNavigationButton(button, selected: false);
            button.Click += NavigationButton_Click;
            _navigationButtons[module.Key] = button;
            _navigationList.Controls.Add(button);
        }

        rolePanel.Controls.Add(usernameLabel);
        rolePanel.Controls.Add(roleLabel);

        navigationPanel.Controls.Add(_navigationList);
        navigationPanel.Controls.Add(UiTheme.CreateDivider());
        navigationPanel.Controls.Add(rolePanel);

        return navigationPanel;
    }

    private Control BuildContentPanel()
    {
        _contentPanel.Dock = DockStyle.Fill;
        _contentPanel.BackColor = UiTheme.Background;
        _contentPanel.Padding = new Padding(16);
        return _contentPanel;
    }

    private void SelectInitialModule()
    {
        ModuleInfo? dashboard = _modules.FirstOrDefault(module => module.Key == "Dashboard");
        ModuleInfo? initialModule = dashboard ?? _modules.FirstOrDefault();

        if (initialModule is not null)
        {
            ShowModule(initialModule.Key);
        }
    }

    private void ShowEmbeddedForm(string moduleKey, object? param = null)
    {
        _contentPanel.SuspendLayout();
        _contentPanel.Controls.Clear();

        Form embeddedForm = moduleKey switch
        {
            "BenhNhan" => new FrmBenhNhan(),
            "TaoLuotKham" => new FrmTaoLuotKham(),
            "HangDoiKham" => CreateHangDoiKham(),
            "KhamBenh" => param is int maLK ? new FrmKhamBenh(maLK, _currentUser.MaNV) : new FrmKhamBenh(0, _currentUser.MaNV),
            "InPhieu" => new FrmInPhieu(),
            "LichSu" => new FrmLichSu(),
            "Dashboard" => new FrmDashboard(),
            _ => throw new InvalidOperationException($"Unknown embeddable module: {moduleKey}")
        };

        embeddedForm.TopLevel = false;
        embeddedForm.FormBorderStyle = FormBorderStyle.None;
        embeddedForm.Dock = DockStyle.Fill;
        _contentPanel.Controls.Add(embeddedForm);
        embeddedForm.Show();
        _contentPanel.ResumeLayout();
    }

    private FrmHangDoiKham CreateHangDoiKham()
    {
        var frm = new FrmHangDoiKham(_currentUser);
        frm.ExamStarted += (s, e) =>
        {
            ShowModule("KhamBenh", e.MaLK);
        };
        return frm;
    }

    private void NavigationButton_Click(object? sender, EventArgs e)
    {
        if (sender is Button button && button.Tag is string moduleKey)
        {
            ShowModule(moduleKey);
        }
    }

    private void ShowModule(string moduleKey, object? param = null)
    {
        ModuleInfo? module = _modules.FirstOrDefault(item => string.Equals(item.Key, moduleKey, StringComparison.OrdinalIgnoreCase));
        if (module is null)
        {
            MessageBox.Show(
                "Chức năng này chưa triển khai trong phiên bản demo.",
                "Thông báo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        UpdateNavigationSelection(module.Key);
        ShowEmbeddedForm(module.Key, param);
    }

    private static void AddInfoRow(TableLayoutPanel body, int rowIndex, string label, string value)
    {
        var labelControl = new Label
        {
            Dock = DockStyle.Fill,
            Font = UiTheme.LabelFont,
            ForeColor = UiTheme.MutedText,
            Text = label,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var valueControl = new Label
        {
            Dock = DockStyle.Fill,
            Font = UiTheme.BodyFont,
            ForeColor = UiTheme.Text,
            Text = value,
            TextAlign = ContentAlignment.MiddleLeft
        };

        body.Controls.Add(labelControl, 0, rowIndex);
        body.Controls.Add(valueControl, 1, rowIndex);
    }

    private void UpdateNavigationSelection(string selectedKey)
    {
        if (_selectedNavigationButton is not null)
        {
            UiTheme.ApplyNavigationButton(_selectedNavigationButton, selected: false);
        }

        if (_navigationButtons.TryGetValue(selectedKey, out Button? selectedButton))
        {
            UiTheme.ApplyNavigationButton(selectedButton, selected: true);
            _selectedNavigationButton = selectedButton;
        }
    }

    private void LogoutButton_Click(object? sender, EventArgs e)
    {
        DialogResult result = MessageBox.Show(
            "Bạn có muốn đăng xuất khỏi hệ thống?",
            "Đăng xuất",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);

        if (result == DialogResult.Yes)
        {
            Close();
        }
    }

    private static List<ModuleInfo> BuildModules(string role)
    {
        if (string.Equals(role, "TiepNhan", StringComparison.OrdinalIgnoreCase))
        {
            return new List<ModuleInfo>
            {
                new("BenhNhan", "Bệnh nhân", "Tìm kiếm, thêm mới và cập nhật hồ sơ bệnh nhân.", "BenhNhanBLL"),
                new("TaoLuotKham", "Tạo lượt khám", "Đăng ký lượt khám, nhận số thứ tự và hủy lượt khi còn chờ.", "LuotKhamBLL"),
                new("LichSu", "Lịch sử", "Tra cứu lịch sử khám theo ngày và từ khóa.", "ThongKeBLL"),
                new("Dashboard", "Dashboard", "Tổng quan nhanh dữ liệu khám trong 7 ngày.", "ThongKeBLL")
            };
        }

        if (string.Equals(role, "BacSi", StringComparison.OrdinalIgnoreCase))
        {
            return new List<ModuleInfo>
            {
                new("HangDoiKham", "Hàng đợi khám", "Theo dõi lượt đang chờ và bắt đầu khám.", "KhamBLL"),
                new("KhamBenh", "Khám bệnh", "Nhập chẩn đoán, toa thuốc và hoàn tất lượt khám.", "KhamBLL"),
                new("InPhieu", "In phiếu", "Xem trước phiếu khám để phục vụ demo không phụ thuộc máy in.", "KhamBLL"),
                new("LichSu", "Lịch sử", "Tra cứu lịch sử khám theo ngày và từ khóa.", "ThongKeBLL"),
                new("Dashboard", "Dashboard", "Tổng quan nhanh dữ liệu khám trong 7 ngày.", "ThongKeBLL")
            };
        }

        return new List<ModuleInfo>
        {
            new("Dashboard", "Dashboard", "Tài khoản chưa có menu nghiệp vụ được cấu hình.", "AuthBLL")
        };
    }

    private static string DisplayName(NhanVienDTO user)
    {
        return string.IsNullOrWhiteSpace(user.HoTen) ? user.Username : user.HoTen;
    }

    private static string RoleDisplayName(string role)
    {
        if (string.Equals(role, "TiepNhan", StringComparison.OrdinalIgnoreCase))
        {
            return "Nhân viên tiếp nhận";
        }

        if (string.Equals(role, "BacSi", StringComparison.OrdinalIgnoreCase))
        {
            return "Bác sĩ";
        }

        return role;
    }

    private sealed record ModuleInfo(string Key, string Title, string Subtitle, string ContractNote);
}
