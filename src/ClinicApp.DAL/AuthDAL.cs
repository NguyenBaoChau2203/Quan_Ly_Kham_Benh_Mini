using System.Data;
using Microsoft.Data.SqlClient;
using ClinicApp.DTO;

namespace ClinicApp.DAL;

public class AuthDAL
{
    public NhanVienDTO? DangNhap(string username, string passwordSha256)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(passwordSha256))
        {
            return null;
        }

        const string query = @"
SELECT TOP (1)
       MaNV,
       Username,
       VaiTro,
       HoTen
FROM NhanVien
WHERE Username = @Username
  AND PasswordHash = @PasswordHash;";

        var parameters = new[]
        {
            new SqlParameter("@Username", SqlDbType.VarChar, 50) { Value = username.Trim() },
            new SqlParameter("@PasswordHash", SqlDbType.VarChar, 64) { Value = passwordSha256 }
        };

        try
        {
            DataTable table = DataProvider.Instance.ExecuteQuery(query, parameters);
            if (table.Rows.Count == 0)
            {
                return null;
            }

            DataRow row = table.Rows[0];
            return new NhanVienDTO
            {
                MaNV = Convert.ToInt32(row["MaNV"]),
                Username = Convert.ToString(row["Username"]) ?? string.Empty,
                Role = Convert.ToString(row["VaiTro"]) ?? string.Empty,
                HoTen = Convert.ToString(row["HoTen"]) ?? string.Empty
            };
        }
        catch
        {
            return null;
        }
    }
}
