namespace ClinicApp.GUI;

using System.Configuration;
using ClinicApp.DAL;
using ClinicApp.GUI.Forms;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Đọc connection string từ App.config và set cho DataProvider
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
        DataProvider.Instance.ConnectionString = connStr;

        try
        {
            using var conn = new Microsoft.Data.SqlClient.SqlConnection(connStr);
            conn.Open();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Không thể kết nối CSDL. Vui lòng kiểm tra lại cấu hình SQL Server.\nChi tiết: {ex.Message}",
                "Lỗi kết nối",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.Run(new FrmLogin());
    }
}
