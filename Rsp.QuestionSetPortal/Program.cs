using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.FeatureManagement;
using Rsp.QuestionSetPortal.Configuration;
using Rsp.QuestionSetPortal.Constants;
using Rsp.QuestionSetPortal.Notifications;
using Rsp.QuestionSetPortal.Services;
using Swashbuckle.AspNetCore.Swagger;
using Umbraco.Cms.Core.Notifications;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("featuresettings.json", true, true);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .AddNotificationHandler<ContentSavingNotification, SavingNotification>()
    .AddNotificationHandler<ContentPublishingNotification, PublishingNotifications>()
    .AddNotificationHandler<ContentMovingToRecycleBinNotification, MovingToRecycleBinNotifications>()
    .AddNotificationHandler<SendingContentNotification, PreviewNotificationHandler>()
    .Build();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<SwaggerOptions>(c =>
{
    c.RouteTemplate = "umbraco/swagger/{documentName}/swagger.json";
});

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddAzureAppConfiguration(builder.Configuration);
}

var settings = builder.Configuration.GetSection(nameof(AppSettings));
builder.Services.Configure<AppSettings>(settings);

var appSettings = settings.Get<AppSettings>()!;

builder.Services.AddSingleton(appSettings);
builder.Services.AddScoped<IRankingCalculationService, RankingCalculationService>();
builder.Services.AddScoped<IModificationQuestionSetService, ModificationQuestionSetService>();

builder.Services.AddFeatureManagement();

var featureManager = new FeatureManager(new ConfigurationFeatureDefinitionProvider(builder.Configuration));

// If the "UseFrontDoor" feature is enabled, configure forwarded headers options
if (await featureManager.IsEnabledAsync(FeatureFlags.UseFrontDoor))
{
    // Configure ForwardedHeadersOptions to handle proxy headers
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        // Specify which forwarded headers should be processed
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                  ForwardedHeaders.XForwardedProto |
                                  ForwardedHeaders.XForwardedHost;

        // Clear known networks and proxies to allow forwarding from any source
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();

        // Set allowed hosts from the AppSettings configuration, splitting by semicolon
        options.AllowedHosts = appSettings.AllowedHosts.Split(';', StringSplitOptions.RemoveEmptyEntries);
    });
}

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

app.UseForwardedHeaders();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseInstallerEndpoints();
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/umbraco/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = "umbraco/swagger";
    });
}

await app.RunAsync();