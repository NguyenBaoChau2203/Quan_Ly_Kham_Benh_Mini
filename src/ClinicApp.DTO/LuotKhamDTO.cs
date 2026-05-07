namespace ClinicApp.DTO;

public class LuotKhamDTO
{
    public int MaLK { get; set; }
    public int MaBN { get; set; }
    public int SoThuTu { get; set; }
    public DateTime NgayKham { get; set; }
    public string TrangThai { get; set; } = string.Empty;
    public int? MaBacSi { get; set; }
}
