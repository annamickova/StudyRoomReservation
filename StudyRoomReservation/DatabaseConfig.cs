using Microsoft.Extensions.Configuration;

public static class DatabaseConfig
{
    public static string ConnectionString { get; private set; } = string.Empty;

    public static void Load()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var dbSection = config.GetSection("Database");

        string server = dbSection["Server"];
        string port = dbSection["Port"];
        string database = dbSection["Database"];
        string user = dbSection["User"];
        string password = dbSection["Password"];

        ConnectionString = $"server={server};port={port};database={database};user={user};password={password}";
    }
}