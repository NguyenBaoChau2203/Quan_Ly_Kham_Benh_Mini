using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicApp.DAL;

public class LuotKhamDAL
{
    public int? TaoLuotKham(int maBN, int? maBacSi, string? ghiChu)
    {
        SqlParameter soThuTu = new("@SoThuTu", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        SqlParameter[] parameters =
        [
            new("@MaBN", SqlDbType.Int) { Value = maBN },
            new("@MaBacSi", SqlDbType.Int) { Value = (object?)maBacSi ?? DBNull.Value },
            new("@GhiChu", SqlDbType.NVarChar, 500) { Value = string.IsNullOrWhiteSpace(ghiChu) ? DBNull.Value : ghiChu.Trim() },
            soThuTu
        ];

        int rowsAffected = DataProvider.Instance.ExecuteNonQuerySP("sp_TaoLuotKham", parameters);
        if (rowsAffected != 1 || soThuTu.Value == DBNull.Value)
        {
            return null;
        }

        int result = Convert.ToInt32(soThuTu.Value);
        return result > 0 ? result : null;
    }

    public bool HuyLuotKham(int maLK)
    {
        const string query = @"
UPDATE LuotKham
SET TrangThai = 'DaHuy'
WHERE MaLK = @MaLK
  AND TrangThai = 'DangCho'";

        SqlParameter[] parameters =
        [
            new("@MaLK", SqlDbType.Int) { Value = maLK }
        ];

        return DataProvider.Instance.ExecuteNonQuery(query, parameters) == 1;
    }
}
