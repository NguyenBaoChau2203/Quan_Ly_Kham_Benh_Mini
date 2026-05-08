using System;
using System.Data;
using ClinicApp.DAL;
using Microsoft.Data.SqlClient;

namespace ClinicApp.BLL
{
    public class DoctorBLL
    {
        private readonly DoctorDAL _doctorDal;

        public DoctorBLL()
        {
            _doctorDal = new DoctorDAL();
        }

        // 1. LẤY HÀNG ĐỢI
        // Yêu cầu: Chỉ lấy trạng thái 'DangCho'. Nếu lỗi không quăng Exception.
        public DataTable LayHangDoiDangCho()
        {
            DataTable dt = _doctorDal.LayHangDoiDangCho();

            // Theo luật demo-safe: Đảm bảo trả về DataTable rỗng nếu DAL bị sự cố (chứ không trả null để GUI văng lỗi)
            if (dt == null)
            {
                return new DataTable();
            }

            return dt;
        }

        // 2. NHẬN BỆNH NHÂN (BẮT ĐẦU KHÁM)
        // Yêu cầu (Quy định 1): Atomic DangCho -> DangKham. Nếu false GUI sẽ báo lượt khám đã bị nhận.
        public bool ChuyenSangDangKham(int maLK, int maBS)
        {
            // Kiểm tra nghiệp vụ (Validate): Mã lượt khám và Mã bác sĩ phải lớn hơn 0
            if (maLK <= 0 || maBS <= 0)
            {
                return false;
            }

            return _doctorDal.ChuyenSangDangKham(maLK, maBS);
        }

        // 3. LƯU KHÁM BỆNH
        // Yêu cầu (Quy định 2): Chống double-save, gọi SP sp_HoanTatKham. 
        public bool HoanTatKham(int maLK, string trieuChung, string chanDoan, string toaThuoc, string loiDan)
        {
            if (maLK <= 0)
            {
                return false;
            }

            // Tiền xử lý dữ liệu: Xóa khoảng trắng thừa ở 2 đầu chuỗi (nếu có) để data sạch sẽ trước khi xuống DB
            trieuChung = trieuChung?.Trim();
            chanDoan = chanDoan?.Trim();
            toaThuoc = toaThuoc?.Trim();
            loiDan = loiDan?.Trim();

            return _doctorDal.HoanTatKham(maLK, trieuChung, chanDoan, toaThuoc, loiDan);
        }

        // 4. QUAY LẠI HÀNG ĐỢI
        // Yêu cầu: Atomic DangKham -> DangCho
        public bool ChuyenVeDangCho(int maLK)
        {
            if (maLK <= 0)
            {
                return false;
            }

            return _doctorDal.ChuyenVeDangCho(maLK);
        }

        // 5. IN PHIẾU KHÁM
        // Yêu cầu (Quy định 3): Cung cấp dữ liệu cho Print Preview, không văng lỗi
        public DataTable LayDuLieuInPhieu(int maLK)
        {
            if (maLK <= 0)
            {
                // Nếu GUI truyền mã sai, BLL chặn ngay và trả về bảng rỗng
                return new DataTable();
            }

            DataTable dt = _doctorDal.LayDuLieuInPhieu(maLK);

            if (dt == null)
            {
                return new DataTable();
            }

            return dt;
        }
    }
}