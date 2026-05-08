using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace ClinicApp.DAL
{
    public class DoctorDAL
    {
        private readonly string connStr =
            ConfigurationManager.ConnectionStrings["ClinicAppDB"].ConnectionString;

        // 1. HÀNG ĐỢI ĐANG CHỜ (CHỈ DangCho)
        public DataTable LayHangDoiDangCho()
        {
            DataTable dt = new DataTable();

            try
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
                        WHERE lk.TrangThai = 'DangCho'
                        ORDER BY lk.SoThuTu";

                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    da.Fill(dt);
                }
            }
            catch
            {
                // ❗ Không throw → trả bảng rỗng
            }

            return dt;
        }

        // 2. ATOMIC: DangCho → DangKham
        public bool ChuyenSangDangKham(int maLK, int maBS)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = @"
                        UPDATE LuotKham
                        SET TrangThai = 'DangKham',
                            MaBacSi = @MaBS
                        WHERE MaLK = @MaLK
                          AND TrangThai = 'DangCho'";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@MaLK", maLK);
                    cmd.Parameters.AddWithValue("@MaBS", maBS);

                    int rows = cmd.ExecuteNonQuery();

                    return rows == 1; //  atomic chuẩn
                }
            }
            catch
            {
                return false;
            }
        }

        // 3. HOÀN TẤT KHÁM (GỌI STORED PROCEDURE)
        public bool HoanTatKham(int maLK, string trieuChung, string chanDoan, string toaThuoc, string loiDan)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("sp_HoanTatKham", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MaLK", maLK);
                    cmd.Parameters.AddWithValue("@TrieuChung", trieuChung ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ChanDoan", chanDoan ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToaThuoc", toaThuoc ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LoiDan", loiDan ?? (object)DBNull.Value);

                    int rows = cmd.ExecuteNonQuery();

                    return rows > 0; // SP fail → trả false
                }
            }
            catch
            {
                return false;
            }
        }

        // 4. QUAY LẠI HÀNG ĐỢI (ATOMIC)
        public bool ChuyenVeDangCho(int maLK)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    string query = @"
                        UPDATE LuotKham
                        SET TrangThai = 'DangCho',
                            MaBacSi = NULL
                        WHERE MaLK = @MaLK
                          AND TrangThai = 'DangKham'";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@MaLK", maLK);

                    int rows = cmd.ExecuteNonQuery();

                    return rows == 1;
                }
            }
            catch
            {
                return false;
            }
        }

        // 5. DỮ LIỆU IN PHIẾU (CHO PRINT PREVIEW)
        public DataTable LayDuLieuInPhieu(int maLK)
        {
            DataTable dt = new DataTable();

            try
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
                            ct.TrieuChung,
                            ct.ChanDoan,
                            ct.ToaThuoc,
                            ct.LoiDan
                        FROM LuotKham lk
                        JOIN BenhNhan bn ON lk.MaBN = bn.MaBN
                        LEFT JOIN NhanVien nv ON lk.MaBacSi = nv.MaNV
                        LEFT JOIN ChiTietKham ct ON lk.MaLK = ct.MaLK
                        WHERE lk.MaLK = @MaLK";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@MaLK", maLK);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            catch
            {
                // trả bảng rỗng
            }

            return dt;
        }
    }
}
