using System.Data;
using ClinicApp.DTO;
using Microsoft.Data.SqlClient;

namespace ClinicApp.DAL;

public class BenhNhanDAL
{
    public DataTable LayDanhSachBenhNhan()
    {
        const string query = @"
SELECT MaBN, HoTen, NgaySinh, GioiTinh, SDT, CCCD, DiaChi
FROM BenhNhan
ORDER BY MaBN DESC";

        return DataProvider.Instance.ExecuteQuery(query);
    }

    public DataTable TimBenhNhan(string? keyword)
    {
        const string query = @"
SELECT MaBN, HoTen, NgaySinh, GioiTinh, SDT, CCCD, DiaChi
FROM BenhNhan
WHERE @Keyword IS NULL
   OR HoTen LIKE @LikeKeyword
   OR SDT LIKE @LikeKeyword
   OR ISNULL(CCCD, '') LIKE @LikeKeyword
ORDER BY MaBN DESC";

        string? trimmedKeyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();
        var parameters = new[]
        {
            new SqlParameter("@Keyword", SqlDbType.NVarChar, 100)
            {
                Value = (object?)trimmedKeyword ?? DBNull.Value
            },
            new SqlParameter("@LikeKeyword", SqlDbType.NVarChar, 102)
            {
                Value = trimmedKeyword == null ? DBNull.Value : $"%{trimmedKeyword}%"
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
        return dt.Rows.Count == 1 ? MapBenhNhan(dt.Rows[0]) : null;
    }

    public bool ThemBenhNhan(BenhNhanDTO bn)
    {
        const string query = @"
INSERT INTO BenhNhan (HoTen, NgaySinh, GioiTinh, SDT, CCCD, DiaChi)
VALUES (@HoTen, @NgaySinh, @GioiTinh, @SDT, @CCCD, @DiaChi);";

        var parameters = new[]
        {
            new SqlParameter("@HoTen", SqlDbType.NVarChar, 100) { Value = bn.HoTen.Trim() },
            new SqlParameter("@NgaySinh", SqlDbType.Date)
            {
                Value = bn.NgaySinh.HasValue ? bn.NgaySinh.Value.Date : DBNull.Value
            },
            new SqlParameter("@GioiTinh", SqlDbType.NVarChar, 10) { Value = bn.GioiTinh.Trim() },
            new SqlParameter("@SDT", SqlDbType.VarChar, 15) { Value = bn.SDT.Trim() },
            new SqlParameter("@CCCD", SqlDbType.VarChar, 12)
            {
                Value = string.IsNullOrWhiteSpace(bn.CCCD) ? DBNull.Value : bn.CCCD.Trim()
            },
            new SqlParameter("@DiaChi", SqlDbType.NVarChar, 200)
            {
                Value = string.IsNullOrWhiteSpace(bn.DiaChi) ? DBNull.Value : bn.DiaChi.Trim()
            }
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
            new SqlParameter("@NgaySinh", SqlDbType.Date)
            {
                Value = bn.NgaySinh.HasValue ? bn.NgaySinh.Value.Date : DBNull.Value
            },
            new SqlParameter("@GioiTinh", SqlDbType.NVarChar, 10) { Value = bn.GioiTinh.Trim() },
            new SqlParameter("@SDT", SqlDbType.VarChar, 15) { Value = bn.SDT.Trim() },
            new SqlParameter("@CCCD", SqlDbType.VarChar, 12)
            {
                Value = string.IsNullOrWhiteSpace(bn.CCCD) ? DBNull.Value : bn.CCCD.Trim()
            },
            new SqlParameter("@DiaChi", SqlDbType.NVarChar, 200)
            {
                Value = string.IsNullOrWhiteSpace(bn.DiaChi) ? DBNull.Value : bn.DiaChi.Trim()
            }
        };

        return DataProvider.Instance.ExecuteNonQuery(query, parameters) == 1;
    }

    public bool TonTaiCCCD(string cccd, int? excludeMaBN = null)
    {
        const string query = @"
SELECT COUNT(1)
FROM BenhNhan
WHERE CCCD = @CCCD
  AND (@ExcludeMaBN IS NULL OR MaBN <> @ExcludeMaBN)";

        var parameters = new[]
        {
            new SqlParameter("@CCCD", SqlDbType.VarChar, 12) { Value = cccd.Trim() },
            new SqlParameter("@ExcludeMaBN", SqlDbType.Int) { Value = (object?)excludeMaBN ?? DBNull.Value }
        };

        object? result = DataProvider.Instance.ExecuteScalar(query, parameters);
        return result != null && Convert.ToInt32(result) > 0;
    }

    public bool TonTaiSDT(string sdt, int? excludeMaBN = null)
    {
        const string query = @"
SELECT COUNT(1)
FROM BenhNhan
WHERE SDT = @SDT
  AND (@ExcludeMaBN IS NULL OR MaBN <> @ExcludeMaBN)";

        var parameters = new[]
        {
            new SqlParameter("@SDT", SqlDbType.VarChar, 15) { Value = sdt.Trim() },
            new SqlParameter("@ExcludeMaBN", SqlDbType.Int) { Value = (object?)excludeMaBN ?? DBNull.Value }
        };

        object? result = DataProvider.Instance.ExecuteScalar(query, parameters);
        return result != null && Convert.ToInt32(result) > 0;
    }

    private static BenhNhanDTO MapBenhNhan(DataRow row)
    {
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
}
