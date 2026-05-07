using System.Data;
using ClinicApp.DAL;
using ClinicApp.DTO;

namespace ClinicApp.BLL;

public class KhamBLL
{
    private readonly KhamDAL _khamDAL = new();

    public DataTable LayHangDoiDangCho()
    {
        return _khamDAL.LayHangDoiDangCho();
    }

    public bool ChuyenSangDangKham(int maLK, int maBacSi)
    {
        return maLK > 0 && maBacSi > 0 && _khamDAL.ChuyenSangDangKham(maLK, maBacSi);
    }

    public bool HoanTatKham(ChiTietKhamDTO ct)
    {
        if (ct.MaLK <= 0 || string.IsNullOrWhiteSpace(ct.ChanDoan))
        {
            return false;
        }

        return _khamDAL.HoanTatKham(ct);
    }

    public bool ChuyenVeDangCho(int maLK)
    {
        return maLK > 0 && _khamDAL.ChuyenVeDangCho(maLK);
    }

    public DataTable LayDuLieuInPhieu(int maLK)
    {
        return maLK <= 0 ? new DataTable() : _khamDAL.LayDuLieuInPhieu(maLK);
    }
}
