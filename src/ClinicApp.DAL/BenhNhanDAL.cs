using System.Data;
using Microsoft.Data.SqlClient;
using ClinicApp.DTO;

namespace ClinicApp.DAL;

public class BenhNhanDAL
{
    public DataTable TimBenhNhan(string? keyword)
    {
        const string query = @"
SELECT MaBN, HoTen, NgaySinh, GioiTinh, SDT, CCCD, DiaChi
FROM BenhNhan
WHERE (@Keyword IS NULL 
       OR HoTen LIKE N'%' + @Keyword + N'%'
       OR SDT LIKE '%' + @Keyword + '%'
       OR CCCD LIKE '%' + @Keyword + '%')
ORDER BY MaBN DESC;";

        var parameters = new[]
        {
            new SqlParameter("@Keyword", SqlDbType.NVarChar, 100) 
            { 
                Value = string.IsNullOrWhiteSpace(keyword) ? DBNull.Value : keyword.Trim() 
            }
        };

        return DataProvider.Instance.ExecuteQuery(query, parameters);
    }

    public BenhNhanDTO? LayBenhNhanTheoMa(int maBN)
    {
        const string query = @"
SELECT MaBN, HoTen, NgaySinh, GioiTinh, SDT, CCCD, DiaChi
FROM BenhNhan
WHERE MaBN = @MaBN;";

        var parameters = new[]
        {
            new SqlParameter("@MaBN", SqlDbType.Int) { Value = maBN }
        };

        DataTable dt = DataProvider.Instance.ExecuteQuery(query, parameters);
        if (dt.Rows.Count == 0) return null;

        DataRow row = dt.Rows[0];
        return new BenhNhanDTO
        {
            MaBN = Convert.ToInt32(row["MaBN"]),
            HoTen = Convert.ToString(row["HoTen"]) ?? string.Empty,
            NgaySinh = row["NgaySinh"] == DBNull.Value ? null : Convert.ToDateTime(row["NgaySinh"]),
            GioiTinh = Convert.ToString(row["GioiTinh"]) ?? string.Empty,
            SDT = Convert.ToString(row["SDT"]) ?? string.Empty,
            CCCD = row["CCCD"] == DBNull.Value ? null : Convert.ToString(row["CCCD"]),
            DiaChi = row["DiaChi"] == DBNull.Value ? null : Convert.ToString(row["DiaChi"])
        };
    }

    public bool ThemBenhNhan(BenhNhanDTO bn)
    {
        const string query = @"
INSERT INTO BenhNhan (HoTen, NgaySinh, GioiTinh, SDT, CCCD, DiaChi)
VALUES (@HoTen, @NgaySinh, @GioiTinh, @SDT, @CCCD, @DiaChi);";

        var parameters = new[]
        {
            new SqlParameter("@HoTen", SqlDbType.NVarChar, 100) { Value = bn.HoTen.Trim() },
            new SqlParameter("@NgaySinh", SqlDbType.Date) { Value = bn.NgaySinh.HasValue ? bn.NgaySinh.Value : DBNull.Value },
            new SqlParameter("@GioiTinh", SqlDbType.NVarChar, 10) { Value = bn.GioiTinh.Trim() },
            new SqlParameter("@SDT", SqlDbType.VarChar, 15) { Value = bn.SDT.Trim() },
            new SqlParameter("@CCCD", SqlDbType.VarChar, 12) { Value = string.IsNullOrWhiteSpace(bn.CCCD) ? DBNull.Value : bn.CCCD.Trim() },
            new SqlParameter("@DiaChi", SqlDbType.NVarChar, 200) { Value = string.IsNullOrWhiteSpace(bn.DiaChi) ? DBNull.Value : bn.DiaChi.Trim() }
        };

        return DataProvider.Instance.ExecuteNonQuery(query, parameters) == 1;
    }

    public bool CapNhatBenhNhan(BenhNhanDTO bn)
    {
        const string query = @"
UPDATE BenhNhan
SET HoTen = @HoTen,
    NgaySinh = @NgaySinh,
    GioiTinh = @GioiTinh,
    SDT = @SDT,
    CCCD = @CCCD,
    DiaChi = @DiaChi
WHERE MaBN = @MaBN;";

        var parameters = new[]
        {
            new SqlParameter("@MaBN", SqlDbType.Int) { Value = bn.MaBN },
            new SqlParameter("@HoTen", SqlDbType.NVarChar, 100) { Value = bn.HoTen.Trim() },
            new SqlParameter("@NgaySinh", SqlDbType.Date) { Value = bn.NgaySinh.HasValue ? bn.NgaySinh.Value : DBNull.Value },
            new SqlParameter("@GioiTinh", SqlDbType.NVarChar, 10) { Value = bn.GioiTinh.Trim() },
            new SqlParameter("@SDT", SqlDbType.VarChar, 15) { Value = bn.SDT.Trim() },
            new SqlParameter("@CCCD", SqlDbType.VarChar, 12) { Value = string.IsNullOrWhiteSpace(bn.CCCD) ? DBNull.Value : bn.CCCD.Trim() },
            new SqlParameter("@DiaChi", SqlDbType.NVarChar, 200) { Value = string.IsNullOrWhiteSpace(bn.DiaChi) ? DBNull.Value : bn.DiaChi.Trim() }
        };

        return DataProvider.Instance.ExecuteNonQuery(query, parameters) == 1;
    }
}
