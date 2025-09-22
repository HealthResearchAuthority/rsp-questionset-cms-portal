namespace Rsp.QuestionSetPortal.Configuration;

public class AppSettings
{
    /// <summary>
    /// Label to use when reading App Configuration from AzureAppConfiguration
    /// </summary>
    public const string ServiceLabel = "cms";

    /// <summary>
    /// Azure App Configuration settings
    /// </summary>
    public AzureAppConfigurations AzureAppConfiguration { get; set; } = null!;

    public Uri? PortalUrl { get; set; }
}