using System.Data;
using ClinicApp.DTO;

namespace ClinicApp.DAL;

public class BenhNhanDAL
{
    public DataTable TimBenhNhan(string? keyword)
    {
        return new DataTable();
    }

    public BenhNhanDTO? LayBenhNhanTheoMa(int maBN)
    {
        return null;
    }

    public bool ThemBenhNhan(BenhNhanDTO bn)
    {
        return false;
    }

    public bool CapNhatBenhNhan(BenhNhanDTO bn)
    {
        return false;
    }
}
