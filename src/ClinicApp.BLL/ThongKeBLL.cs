using System.Data;
using ClinicApp.DAL;

namespace ClinicApp.BLL;

public class ThongKeBLL
{
    private readonly ThongKeDAL _thongKeDAL = new();

    public DataTable LayLichSuKham(DateTime fromDate, DateTime toDate, string? keyword)
    {
        return fromDate > toDate ? new DataTable() : _thongKeDAL.LayLichSuKham(fromDate, toDate, keyword);
    }

    public DataTable LayThongKe7Ngay()
    {
        return _thongKeDAL.LayThongKe7Ngay();
    }
}
