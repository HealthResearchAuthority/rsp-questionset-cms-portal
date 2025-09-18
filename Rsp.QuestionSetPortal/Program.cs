using Rsp.QuestionSetPortal.Notifications;
using Swashbuckle.AspNetCore.Swagger;
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
    .AddNotificationHandler<SendingContentNotification, PreviewNotificationNotificationHandler>()
    .Build();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<SwaggerOptions>(c =>
{
    c.RouteTemplate = "umbraco/swagger/{documentName}/swagger.json";
});

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