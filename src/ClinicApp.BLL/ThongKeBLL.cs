using System.Data;
using ClinicApp.DAL;

namespace ClinicApp.BLL;

public class ThongKeBLL
{
    private readonly ThongKeDAL _thongKeDAL = new();

    public DataTable LayLichSuKham(DateTime fromDate, DateTime toDate, string? keyword)
    {
        if (fromDate > toDate)
        {
            return new DataTable();
        }

        string? normalizedKeyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();
        return _thongKeDAL.LayLichSuKham(fromDate, toDate, normalizedKeyword);
    }

    public DataTable LayThongKe7Ngay()
    {
        return _thongKeDAL.LayThongKe7Ngay();
    }
}
