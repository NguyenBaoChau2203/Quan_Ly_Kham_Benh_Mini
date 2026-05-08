using ClinicApp.DAL;

namespace ClinicApp.BLL;

public static class AppBootstrap
{
    public static void Initialize(string connectionString)
    {
        DataProvider.Instance.ConnectionString = connectionString;
    }

    public static bool TestConnection(string connectionString, out string errorMessage)
    {
        errorMessage = string.Empty;
        try
        {
            using var conn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            conn.Open();
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }
}
