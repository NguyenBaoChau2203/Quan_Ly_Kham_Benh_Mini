using System.Security.Cryptography;
using System.Text;
using ClinicApp.DAL;
using ClinicApp.DTO;

namespace ClinicApp.BLL;

public class AuthBLL
{
    private readonly AuthDAL _authDAL = new();

    public NhanVienDTO? DangNhap(string username, string passwordPlain)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(passwordPlain))
        {
            return null;
        }

        string passwordSha256 = HashSha256(passwordPlain);
        return _authDAL.DangNhap(username.Trim(), passwordSha256);
    }

    private static string HashSha256(string value)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
