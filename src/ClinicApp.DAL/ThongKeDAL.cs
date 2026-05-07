using System.Data;
using Microsoft.Data.SqlClient;

namespace ClinicApp.DAL;

public class ThongKeDAL
{
    public DataTable LayLichSuKham(DateTime fromDate, DateTime toDate, string? keyword)
    {
        const string query = @"
SELECT
    lk.MaLK,
    lk.NgayKham,
    lk.MaBN,
    bn.HoTen,
    ISNULL(nv.HoTen, N'') AS TenBacSi,
    ISNULL(ct.ChanDoan, N'') AS ChanDoan,
    lk.TrangThai
FROM LuotKham lk
INNER JOIN BenhNhan bn ON lk.MaBN = bn.MaBN
LEFT JOIN NhanVien nv ON lk.MaBacSi = nv.MaNV
LEFT JOIN ChiTietKham ct ON lk.MaLK = ct.MaLK
WHERE lk.NgayKhamDate >= @FromDate
  AND lk.NgayKhamDate <= @ToDate
  AND (@Keyword IS NULL
       OR bn.HoTen LIKE N'%' + @Keyword + N'%'
       OR bn.SDT LIKE '%' + @Keyword + '%'
       OR bn.CCCD LIKE '%' + @Keyword + '%')
ORDER BY lk.NgayKham DESC, lk.MaLK DESC;";

        var parameters = new[]
        {
            new SqlParameter("@FromDate", SqlDbType.Date) { Value = fromDate.Date },
            new SqlParameter("@ToDate", SqlDbType.Date) { Value = toDate.Date },
            new SqlParameter("@Keyword", SqlDbType.NVarChar, 200)
            {
                Value = string.IsNullOrWhiteSpace(keyword) ? DBNull.Value : keyword.Trim()
            }
        };

        return DataProvider.Instance.ExecuteQuery(query, parameters);
    }

    public DataTable LayThongKe7Ngay()
    {
        const string query = @"
WITH DayList AS (
    SELECT Ngay FROM (VALUES
        (CAST(GETDATE() AS DATE)),
        (DATEADD(DAY, -1, CAST(GETDATE() AS DATE))),
        (DATEADD(DAY, -2, CAST(GETDATE() AS DATE))),
        (DATEADD(DAY, -3, CAST(GETDATE() AS DATE))),
        (DATEADD(DAY, -4, CAST(GETDATE() AS DATE))),
        (DATEADD(DAY, -5, CAST(GETDATE() AS DATE))),
        (DATEADD(DAY, -6, CAST(GETDATE() AS DATE)))
    ) AS v(Ngay)
)
SELECT
    dl.Ngay,
    COUNT(lk.MaLK) AS SoLuot,
    SUM(CASE WHEN lk.TrangThai = 'DaKham' THEN 1 ELSE 0 END) AS SoDaKham,
    SUM(CASE WHEN lk.TrangThai = 'DangCho' THEN 1 ELSE 0 END) AS SoDangCho
FROM DayList dl
LEFT JOIN LuotKham lk ON lk.NgayKhamDate = dl.Ngay
GROUP BY dl.Ngay
ORDER BY dl.Ngay DESC;";

        return DataProvider.Instance.ExecuteQuery(query);
    }
}
