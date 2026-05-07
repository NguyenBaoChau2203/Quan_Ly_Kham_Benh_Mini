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
        return IsValidBenhNhan(bn) && _benhNhanDAL.ThemBenhNhan(bn);
    }

    public bool CapNhatBenhNhan(BenhNhanDTO bn)
    {
        return bn.MaBN > 0 && IsValidBenhNhan(bn) && _benhNhanDAL.CapNhatBenhNhan(bn);
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
