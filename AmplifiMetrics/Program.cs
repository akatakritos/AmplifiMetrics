using System.Net;
using AmplifiMetrics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

var cookieContainer = new CookieContainer();
builder.Services
    .AddHttpClient<AmplifiClient, AmplifiClient>(client =>
        new AmplifiClient(client, builder.Configuration["AmplifiMetricsRouterPassword"]!, cookieContainer))
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
    {
        CookieContainer = cookieContainer,
    })
    .ConfigureHttpClient(client =>
    {
        client.BaseAddress = new Uri("http://" + builder.Configuration["AmplifiMetricsDefaultGateway"]);
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.MapGet("/metrics", async (AmplifiClient client) =>
{
    ArgumentNullException.ThrowIfNull(client);
    var metricsJson = await client.GetMetrics();
    var metrics = MetricConverter.Parse(metricsJson);
    return PrometheusRenderer.Render(metrics);
});

app.Run();
