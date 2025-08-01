using Rsp.QuestionSetPortal.Notifications;
using Umbraco.Cms.Core.Notifications;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .AddNotificationHandler<ContentSavingNotification, SavingNotification>()
    .AddNotificationHandler<ContentPublishingNotification, PublishingNotifications>()
    .AddNotificationHandler<ContentMovingToRecycleBinNotification, MovingToRecycleBinNotifications>()
    .Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

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

await app.RunAsync();