namespace ClinicApp.GUI;

using System.Configuration;
using ClinicApp.BLL;
using ClinicApp.GUI.Forms;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Đọc connection string từ App.config và set qua BLL
        string? connStr = ConfigurationManager.ConnectionStrings["ClinicAppDB"]?.ConnectionString;
        if (string.IsNullOrEmpty(connStr))
        {
            MessageBox.Show(
                "Không tìm thấy connection string 'ClinicAppDB' trong App.config.\nVui lòng kiểm tra lại cấu hình.",
                "Lỗi cấu hình",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        AppBootstrap.Initialize(connStr);

        if (!AppBootstrap.TestConnection(connStr, out string errorMessage))
        {
            MessageBox.Show(
                $"Không thể kết nối CSDL. Vui lòng kiểm tra lại cấu hình SQL Server.\nChi tiết: {errorMessage}",
                "Lỗi kết nối",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        ApplicationConfiguration.Initialize();
        Application.Run(new FrmLogin());
    }
}
