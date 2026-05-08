using System.Data;
using ClinicApp.DAL;
using ClinicApp.DTO;

namespace ClinicApp.BLL;

public class BenhNhanBLL
{
    private readonly BenhNhanDAL _benhNhanDAL = new();

    public DataTable TimBenhNhan(string? keyword)
    {
        return _benhNhanDAL.TimBenhNhan(keyword);
    }

    public BenhNhanDTO? LayBenhNhanTheoMa(int maBN)
    {
        return maBN <= 0 ? null : _benhNhanDAL.LayBenhNhanTheoMa(maBN);
    }

    public bool ThemBenhNhan(BenhNhanDTO bn)
    {
        if (!IsValidBenhNhan(bn)) return false;
        if (_benhNhanDAL.TonTaiSDT(bn.SDT)) return false;
        if (!string.IsNullOrWhiteSpace(bn.CCCD) && _benhNhanDAL.TonTaiCCCD(bn.CCCD)) return false;
        return _benhNhanDAL.ThemBenhNhan(bn);
    }

    public bool CapNhatBenhNhan(BenhNhanDTO bn)
    {
        if (bn.MaBN <= 0 || !IsValidBenhNhan(bn)) return false;
        if (_benhNhanDAL.TonTaiSDT(bn.SDT, bn.MaBN)) return false;
        if (!string.IsNullOrWhiteSpace(bn.CCCD) && _benhNhanDAL.TonTaiCCCD(bn.CCCD, bn.MaBN)) return false;
        return _benhNhanDAL.CapNhatBenhNhan(bn);
    }

    private static bool IsValidBenhNhan(BenhNhanDTO bn)
    {
        if (string.IsNullOrWhiteSpace(bn.HoTen) || string.IsNullOrWhiteSpace(bn.SDT))
        {
            return false;
        }

        return string.IsNullOrWhiteSpace(bn.CCCD) || (bn.CCCD.Length == 12 && bn.CCCD.All(char.IsDigit));
    }
}
