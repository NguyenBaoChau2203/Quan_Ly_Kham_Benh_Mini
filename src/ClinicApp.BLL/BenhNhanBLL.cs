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

    public DataTable LayDanhSachBenhNhan()
    {
        return _benhNhanDAL.LayDanhSachBenhNhan();
    }

    public BenhNhanDTO? LayBenhNhanTheoMa(int maBN)
    {
        return maBN <= 0 ? null : _benhNhanDAL.LayBenhNhanTheoMa(maBN);
    }

    public Tuple<bool, string> ThemBenhNhan(BenhNhanDTO bn)
    {
        if (!IsValidBenhNhan(bn))
        {
            return Tuple.Create(false, "INVALID_DATA");
        }

        string? cccd = NormalizeOptional(bn.CCCD);
        if (cccd != null && _benhNhanDAL.TonTaiCCCD(cccd))
        {
            return Tuple.Create(false, "CCCD_EXIST");
        }

        bool phoneExists = _benhNhanDAL.TonTaiSDT(bn.SDT);
        bool inserted = _benhNhanDAL.ThemBenhNhan(NormalizeBenhNhan(bn));
        if (!inserted)
        {
            return Tuple.Create(false, cccd != null && _benhNhanDAL.TonTaiCCCD(cccd) ? "CCCD_EXIST" : "INSERT_FAILED");
        }

        return Tuple.Create(true, phoneExists ? "PHONE_EXIST_WARNING" : "SUCCESS");
    }

    public bool CapNhatBenhNhan(BenhNhanDTO bn)
    {
        return bn.MaBN > 0 && IsValidBenhNhan(bn) && _benhNhanDAL.CapNhatBenhNhan(NormalizeBenhNhan(bn));
    }

    private static bool IsValidBenhNhan(BenhNhanDTO bn)
    {
        if (string.IsNullOrWhiteSpace(bn.HoTen) || string.IsNullOrWhiteSpace(bn.SDT))
        {
            return false;
        }

        string? cccd = NormalizeOptional(bn.CCCD);
        return cccd == null || (cccd.Length == 12 && cccd.All(char.IsDigit));
    }

    private static BenhNhanDTO NormalizeBenhNhan(BenhNhanDTO bn)
    {
        return new BenhNhanDTO
        {
            MaBN = bn.MaBN,
            HoTen = bn.HoTen.Trim(),
            NgaySinh = bn.NgaySinh,
            GioiTinh = string.IsNullOrWhiteSpace(bn.GioiTinh) ? "Nam" : bn.GioiTinh.Trim(),
            SDT = bn.SDT.Trim(),
            CCCD = NormalizeOptional(bn.CCCD),
            DiaChi = NormalizeOptional(bn.DiaChi)
        };
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
