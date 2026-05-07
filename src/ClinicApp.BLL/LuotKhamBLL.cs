using ClinicApp.DAL;

namespace ClinicApp.BLL;

public class LuotKhamBLL
{
    private readonly LuotKhamDAL _luotKhamDAL = new();

    public int? TaoLuotKham(int maBN, int? maBacSi, string? ghiChu)
    {
        return maBN <= 0 ? null : _luotKhamDAL.TaoLuotKham(maBN, maBacSi, ghiChu);
    }

    public bool HuyLuotKham(int maLK)
    {
        return maLK > 0 && _luotKhamDAL.HuyLuotKham(maLK);
    }
}
