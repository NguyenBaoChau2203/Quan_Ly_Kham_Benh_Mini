using System.Data;
using Microsoft.Data.SqlClient;

namespace ClinicApp.DAL;

/// <summary>
/// Singleton trung tâm để thực thi SQL.
/// Mọi DAL gọi qua đây, không tự mở SqlConnection.
/// Lỗi được log vào %LocalAppData%\ClinicApp\error.log.
/// </summary>
public sealed class DataProvider
{
    public static DataProvider Instance { get; } = new();

    private DataProvider()
    {
    }

    /// <summary>
    /// Connection string đọc từ App.config, set bởi Program.cs khi khởi động.
    /// </summary>
    public string? ConnectionString { get; set; }

    // =========================================================
    // 1. Truy vấn trả DataTable (SELECT)
    // =========================================================

    /// <summary>
    /// Chạy câu SELECT, trả DataTable. Trả bảng rỗng nếu lỗi.
    /// </summary>
    public DataTable ExecuteQuery(string query, SqlParameter[]? parameters = null)
    {
        var dt = new DataTable();
        try
        {
            if (string.IsNullOrEmpty(ConnectionString)) throw new InvalidOperationException("ConnectionString has not been initialized.");
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(query, conn) { CommandType = CommandType.Text };
            AddParameters(cmd, parameters);
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
        }
        catch (Exception ex)
        {
            LogError(ex, query);
        }
        return dt;
    }

    /// <summary>
    /// Chạy stored procedure trả DataTable (ví dụ: load danh sách).
    /// </summary>
    public DataTable ExecuteQuerySP(string spName, SqlParameter[]? parameters = null)
    {
        var dt = new DataTable();
        try
        {
            if (string.IsNullOrEmpty(ConnectionString)) throw new InvalidOperationException("ConnectionString has not been initialized.");
            using var conn = new SqlConnection(ConnectionString);
            using var cmd = new SqlCommand(spName, conn) { CommandType = CommandType.StoredProcedure };
            AddParameters(cmd, parameters);
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
        }
        catch (Exception ex)
        {
            LogError(ex, spName);
        }
        return dt;
    }

    // =========================================================
    // 2. Thực thi không trả dữ liệu (INSERT/UPDATE/DELETE)
    // =========================================================

    /// <summary>
    /// Chạy câu INSERT/UPDATE/DELETE, trả số dòng bị ảnh hưởng.
    /// Trả 0 nếu lỗi (DAL check RowsAffected == 1 để biết thành công).
    /// </summary>
    public int ExecuteNonQuery(string query, SqlParameter[]? parameters = null)
    {
        try
        {
            if (string.IsNullOrEmpty(ConnectionString)) throw new InvalidOperationException("ConnectionString has not been initialized.");
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand(query, conn) { CommandType = CommandType.Text };
            AddParameters(cmd, parameters);
            return cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            LogError(ex, query);
            return 0;
        }
    }

    /// <summary>
    /// Chạy stored procedure không trả DataTable (ví dụ: sp_TaoLuotKham, sp_HoanTatKham).
    /// OUTPUT parameters sẽ được cập nhật trong mảng SqlParameter[] sau khi chạy.
    /// Trả số dòng bị ảnh hưởng, 0 nếu lỗi.
    /// </summary>
    public int ExecuteNonQuerySP(string spName, SqlParameter[]? parameters = null)
    {
        try
        {
            if (string.IsNullOrEmpty(ConnectionString)) throw new InvalidOperationException("ConnectionString has not been initialized.");
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand(spName, conn) { CommandType = CommandType.StoredProcedure };
            AddParameters(cmd, parameters);
            return cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            LogError(ex, spName);
            return 0;
        }
    }

    // =========================================================
    // 3. Trả giá trị đơn (COUNT, MAX, etc.)
    // =========================================================

    /// <summary>
    /// Chạy câu truy vấn trả giá trị đơn. Trả null nếu lỗi.
    /// </summary>
    public object? ExecuteScalar(string query, SqlParameter[]? parameters = null)
    {
        try
        {
            if (string.IsNullOrEmpty(ConnectionString)) throw new InvalidOperationException("ConnectionString has not been initialized.");
            using var conn = new SqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new SqlCommand(query, conn) { CommandType = CommandType.Text };
            AddParameters(cmd, parameters);
            return cmd.ExecuteScalar();
        }
        catch (Exception ex)
        {
            LogError(ex, query);
            return null;
        }
    }

    // =========================================================
    // Helpers
    // =========================================================

    private static void AddParameters(SqlCommand cmd, SqlParameter[]? parameters)
    {
        if (parameters == null) return;
        foreach (var p in parameters)
        {
            cmd.Parameters.Add(p);
        }
    }

    private static void LogError(Exception ex, string context)
    {
        try
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ClinicApp");
            Directory.CreateDirectory(folder);

            string logPath = Path.Combine(folder, "error.log");
            string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{context}] {ex.GetType().Name}: {ex.Message}{Environment.NewLine}";
            File.AppendAllText(logPath, entry);
        }
        catch
        {
            // Không để lỗi ghi log làm crash app
        }
    }
}
