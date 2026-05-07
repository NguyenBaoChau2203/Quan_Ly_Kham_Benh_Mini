using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Configuration;
using System.Data;

namespace ClinicApp.DAL
{
    public class DoctorDAL
    {
        private readonly string connStr;

        public DoctorDAL()
        {
            connStr = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
        }

        // 1. Hàng đợi DangCho
        public DataTable LayHangDoiDangCho()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT 
                        lk.MaLK,
                        lk.SoThuTu,
                        bn.MaBN,
                        bn.HoTen,
                        lk.NgayKham,
                        DATEDIFF(MINUTE, lk.NgayKham, GETDATE()) AS ThoiGianChoPhut,
                        lk.TrangThai
                    FROM LuotKham lk
                    JOIN BenhNhan bn ON lk.MaBN = bn.MaBN
                    WHERE lk.TrangThai = N'DangCho'
                    ORDER BY lk.SoThuTu";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // 2. Atomic: DangCho → DangKham
        public bool ChuyenSangDangKham(int maLK, int maBS)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"
                    UPDATE LuotKham
                    SET TrangThai = N'DangKham',
                        MaBacSi = @MaBS
                    WHERE MaLK = @MaLK
                      AND TrangThai = N'DangCho'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@MaLK", SqlDbType.Int).Value = maLK;
                    cmd.Parameters.Add("@MaBS", SqlDbType.Int).Value = maBS;

                    int rows = cmd.ExecuteNonQuery();
                    return rows == 1; // 🔥 atomic check
                }
            }
        }

        // 3. Hoàn tất khám (Stored Procedure + chống double save)
        public bool HoanTatKham(int maLK, string trieuChung, string chanDoan, string toaThuoc, string loiDan)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_HoanTatKham", conn, tran))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.Add("@MaLK", SqlDbType.Int).Value = maLK;
                            cmd.Parameters.Add("@TrieuChung", SqlDbType.NVarChar).Value = trieuChung ?? (object)DBNull.Value;
                            cmd.Parameters.Add("@ChanDoan", SqlDbType.NVarChar).Value = chanDoan ?? (object)DBNull.Value;
                            cmd.Parameters.Add("@ToaThuoc", SqlDbType.NVarChar).Value = toaThuoc ?? (object)DBNull.Value;
                            cmd.Parameters.Add("@LoiDan", SqlDbType.NVarChar).Value = loiDan ?? (object)DBNull.Value;

                            int rows = cmd.ExecuteNonQuery();

                            if (rows <= 0)
                            {
                                tran.Rollback();
                                return false;
                            }

                            tran.Commit();
                            return true;
                        }
                    }
                    catch
                    {
                        tran.Rollback();
                        return false;
                    }
                }
            }
        }

        // 4. Quay lại DangCho (atomic)
        public bool ChuyenVeDangCho(int maLK)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string query = @"
                    UPDATE LuotKham
                    SET TrangThai = N'DangCho'
                    WHERE MaLK = @MaLK
                      AND TrangThai = N'DangKham'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@MaLK", SqlDbType.Int).Value = maLK;

                    int rows = cmd.ExecuteNonQuery();
                    return rows == 1;
                }
            }
        }

        // 5. Dữ liệu in phiếu
        public DataTable LayDuLieuInPhieu(int maLK)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                    SELECT 
                        lk.MaLK,
                        lk.SoThuTu,
                        lk.NgayKham,
                        bn.MaBN,
                        bn.HoTen,
                        bn.NgaySinh,
                        bn.GioiTinh,
                        nv.HoTen AS TenBacSi,
                        ct.ChanDoan,
                        ct.ToaThuoc,
                        ct.LoiDan
                    FROM LuotKham lk
                    JOIN BenhNhan bn ON lk.MaBN = bn.MaBN
                    LEFT JOIN NhanVien nv ON lk.MaBacSi = nv.MaNV
                    LEFT JOIN ChiTietKham ct ON lk.MaLK = ct.MaLK
                    WHERE lk.MaLK = @MaLK";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@MaLK", SqlDbType.Int).Value = maLK;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    return dt;
                }
            }
        }
    }
}