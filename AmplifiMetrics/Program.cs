using AmplifiMetrics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services
    .AddHttpClient<AmplifiClient, AmplifiClient>(client => new AmplifiClient(client, builder.Configuration["AmplifiMetricsDefaultGateway"]!))
    .AddHttpMessageHandler(() => new AmplifiAuthHandler(builder.Configuration["AmplifiMetricsRouterPassword"]!));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}

app.MapGet("/metrics", (AmplifiClient client) =>
{
    ArgumentNullException.ThrowIfNull(client);
    return "Hello World!";
});

app.Run();
