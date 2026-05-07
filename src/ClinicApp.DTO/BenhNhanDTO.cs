namespace ClinicApp.DTO;

public class BenhNhanDTO
{
    public int MaBN { get; set; }
    public string HoTen { get; set; } = string.Empty;
    public DateTime? NgaySinh { get; set; }
    public string GioiTinh { get; set; } = string.Empty;
    public string SDT { get; set; } = string.Empty;
    public string? CCCD { get; set; }
    public string? DiaChi { get; set; }
}
