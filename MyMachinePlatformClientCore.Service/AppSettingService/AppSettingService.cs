using Microsoft.Extensions.Configuration;

namespace MyMachinePlatformClientCore.Service.AppSettingService;

public class AppSettingService
{
    private static IConfiguration _configuration;
 
    

    public AppSettingService()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"appsettings.json"), optional: true, reloadOnChange: true);
        _configuration = builder.Build();
    }

    public static string GetConnectionString(string name)
    {
        return _configuration.GetConnectionString(name);
    }

    
}