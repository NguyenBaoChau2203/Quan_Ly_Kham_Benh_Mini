using System;
using System.Data;
using Microsoft.Data.SqlClient;
using ClinicApp.DTO;

namespace ClinicApp.DAL;

public class KhamDAL
{
    public DataTable LayHangDoiDangCho()
    {
        const string query = @"
SELECT 
    lk.MaLK,
    lk.SoThuTu,
    lk.MaBN,
    bn.HoTen,
    lk.NgayKham,
    DATEDIFF(MINUTE, lk.NgayKham, GETDATE()) AS ThoiGianChoPhut,
    lk.TrangThai
FROM LuotKham lk
INNER JOIN BenhNhan bn ON lk.MaBN = bn.MaBN
WHERE lk.TrangThai = 'DangCho'
  AND lk.NgayKhamDate = CAST(GETDATE() AS DATE)
ORDER BY lk.SoThuTu ASC;";

        return DataProvider.Instance.ExecuteQuery(query);
    }

    public bool ChuyenSangDangKham(int maLK, int maBacSi)
    {
        var parameters = new[]
        {
            new SqlParameter("@MaLK", SqlDbType.Int) { Value = maLK },
            new SqlParameter("@MaBacSi", SqlDbType.Int) { Value = maBacSi },
            new SqlParameter("@KetQua", SqlDbType.Int) { Direction = ParameterDirection.Output }
        };

        DataProvider.Instance.ExecuteNonQuerySP("sp_ChuyenSangDangKham", parameters);

        return parameters[2].Value != DBNull.Value && Convert.ToInt32(parameters[2].Value) == 1;
    }

    public bool HoanTatKham(ChiTietKhamDTO ct)
    {
        var parameters = new[]
        {
            new SqlParameter("@MaLK", SqlDbType.Int) { Value = ct.MaLK },
            new SqlParameter("@TrieuChung", SqlDbType.NVarChar, 1000) { Value = string.IsNullOrWhiteSpace(ct.TrieuChung) ? DBNull.Value : ct.TrieuChung.Trim() },
            new SqlParameter("@ChanDoan", SqlDbType.NVarChar, 1000) { Value = ct.ChanDoan.Trim() },
            new SqlParameter("@ToaThuoc", SqlDbType.NVarChar, 1000) { Value = string.IsNullOrWhiteSpace(ct.ToaThuoc) ? DBNull.Value : ct.ToaThuoc.Trim() },
            new SqlParameter("@LoiDan", SqlDbType.NVarChar, 1000) { Value = string.IsNullOrWhiteSpace(ct.LoiDan) ? DBNull.Value : ct.LoiDan.Trim() },
            new SqlParameter("@KetQua", SqlDbType.Int) { Direction = ParameterDirection.Output }
        };

        DataProvider.Instance.ExecuteNonQuerySP("sp_HoanTatKham", parameters);

        return parameters[5].Value != DBNull.Value && Convert.ToInt32(parameters[5].Value) == 1;
    }

    public bool ChuyenVeDangCho(int maLK)
    {
        const string query = @"
UPDATE LuotKham
SET TrangThai = 'DangCho',
    MaBacSi = NULL
WHERE MaLK = @MaLK AND TrangThai = 'DangKham';";

        var parameters = new[]
        {
            new SqlParameter("@MaLK", SqlDbType.Int) { Value = maLK }
        };

        return DataProvider.Instance.ExecuteNonQuery(query, parameters) == 1;
    }

    public DataTable LayDuLieuInPhieu(int maLK)
    {
        const string query = @"
SELECT 
    lk.MaLK,
    lk.SoThuTu,
    lk.NgayKham,
    lk.MaBN,
    bn.HoTen,
    bn.NgaySinh,
    bn.GioiTinh,
    ISNULL(nv.HoTen, N'') AS TenBacSi,
    lk.GhiChu,
    ISNULL(ct.TrieuChung, N'') AS TrieuChung,
    ISNULL(ct.ChanDoan, N'') AS ChanDoan,
    ISNULL(ct.ToaThuoc, N'') AS ToaThuoc,
    ISNULL(ct.LoiDan, N'') AS LoiDan
FROM LuotKham lk
INNER JOIN BenhNhan bn ON lk.MaBN = bn.MaBN
LEFT JOIN NhanVien nv ON lk.MaBacSi = nv.MaNV
LEFT JOIN ChiTietKham ct ON lk.MaLK = ct.MaLK
WHERE lk.MaLK = @MaLK;";

        var parameters = new[]
        {
            new SqlParameter("@MaLK", SqlDbType.Int) { Value = maLK }
        };

        return DataProvider.Instance.ExecuteQuery(query, parameters);
    }
}
