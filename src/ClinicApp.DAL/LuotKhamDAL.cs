using System;
using System.Data;
using ClinicApp.DTO;
using Microsoft.Data.SqlClient;

namespace ClinicApp.DAL;

public class LuotKhamDAL
{
    public LuotKhamDTO? TaoLuotKham(int maBN, int? maBacSi, string? ghiChu)
    {
        var parameters = new[]
        {
            new SqlParameter("@MaBN", SqlDbType.Int) { Value = maBN },
            new SqlParameter("@MaBacSi", SqlDbType.Int) { Value = maBacSi.HasValue ? maBacSi.Value : DBNull.Value },
            new SqlParameter("@GhiChu", SqlDbType.NVarChar, 500) { Value = string.IsNullOrWhiteSpace(ghiChu) ? DBNull.Value : ghiChu.Trim() },
            new SqlParameter("@MaLK", SqlDbType.Int) { Direction = ParameterDirection.Output },
            new SqlParameter("@SoThuTu", SqlDbType.Int) { Direction = ParameterDirection.Output },
            new SqlParameter("@NgayKham", SqlDbType.DateTime) { Direction = ParameterDirection.Output }
        };

        DataProvider.Instance.ExecuteNonQuerySP("sp_TaoLuotKham", parameters);

        int maLK = parameters[3].Value != DBNull.Value ? Convert.ToInt32(parameters[3].Value) : 0;
        int soThuTu = parameters[4].Value != DBNull.Value ? Convert.ToInt32(parameters[4].Value) : 0;

        if (maLK <= 0) return null;

        DateTime ngayKham = parameters[5].Value != DBNull.Value ? Convert.ToDateTime(parameters[5].Value) : DateTime.Now;

        return new LuotKhamDTO
        {
            MaLK = maLK,
            MaBN = maBN,
            SoThuTu = soThuTu,
            NgayKham = ngayKham,
            TrangThai = "DangCho",
            MaBacSi = maBacSi,
            GhiChu = ghiChu
        };
    }

    public bool HuyLuotKham(int maLK)
    {
        const string query = @"
UPDATE LuotKham
SET TrangThai = 'DaHuy'
WHERE MaLK = @MaLK AND TrangThai = 'DangCho';";

        var parameters = new[]
        {
            new SqlParameter("@MaLK", SqlDbType.Int) { Value = maLK }
        };

        return DataProvider.Instance.ExecuteNonQuery(query, parameters) == 1;
    }
}
