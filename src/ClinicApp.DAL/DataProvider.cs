namespace ClinicApp.DAL;

public sealed class DataProvider
{
    public static DataProvider Instance { get; } = new();

    private DataProvider()
    {
    }

    public string? ConnectionString { get; set; }
}
