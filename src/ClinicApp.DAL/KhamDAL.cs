using System.Data;
using ClinicApp.DTO;

namespace ClinicApp.DAL;

public class KhamDAL
{
    public DataTable LayHangDoiDangCho()
    {
        return new DataTable();
    }

    public bool ChuyenSangDangKham(int maLK, int maBacSi)
    {
        return false;
    }

    public bool HoanTatKham(ChiTietKhamDTO ct)
    {
        return false;
    }

    public bool ChuyenVeDangCho(int maLK)
    {
        return false;
    }

    public DataTable LayDuLieuInPhieu(int maLK)
    {
        return new DataTable();
    }
}
