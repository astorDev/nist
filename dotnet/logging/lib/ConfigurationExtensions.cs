namespace Nist.Logs;

public static class ConfigurationExtensions
{
    public static void SetNistLogLevels(this IConfiguration configuration) {
        configuration["Logging:LogLevel:Default"] = "Warning";
        configuration["Logging:LogLevel:Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware"] = "None";
        configuration["Logging:LogLevel:Microsoft.Hosting.Lifetime"] = "Information";
        configuration["Logging:StateJsonConsole:LogLevel:Default"] = "None";
        configuration["Logging:StateJsonConsole:LogLevel:Nist.Logs"] = "Information";
    }
}