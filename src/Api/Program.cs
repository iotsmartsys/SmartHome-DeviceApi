using Api.Models;
using Core.DI;
using Data.Repositories.MySql.DI;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using System.Diagnostics;

var cts = new CancellationTokenSource();

var builder = WebApplication.CreateBuilder(args);
// ==== OpenTelemetry / Grafana Cloud configuration ====
// Preferir as variáveis padrão OTEL_* do ambiente. Se quiser, você pode
// definir GRAFANA_OTLP_ENDPOINT/GRAFANA_OTLP_TOKEN e adaptar abaixo.

string? connectionString = builder.Configuration.GetConnectionString("Devices");
builder.Services.AddOpenApi();
// Identify this service in telemetry
var serviceName = "SmartHome-Api";
var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";
var resource = ResourceBuilder.CreateDefault()
    .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
    .AddAttributes(new[]
    {
        new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName),
        new KeyValuePair<string, object>("service.namespace", "iot-smart-home"),
        new KeyValuePair<string, object>("host.name", Environment.MachineName)
    });

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName: serviceName, serviceVersion: serviceVersion)
        .AddAttributes(new[]
        {
            new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName),
            new KeyValuePair<string, object>("service.namespace", "iot-smart-home"),
            new KeyValuePair<string, object>("host.name", Environment.MachineName)
        }))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation(o =>
        {
            o.RecordException = true;
        })
        .AddHttpClientInstrumentation(o =>
        {
            o.RecordException = true;
        })
        .AddSource("MySqlConnector")
        .AddOtlpExporter())
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
    .AddRuntimeInstrumentation()
        .AddOtlpExporter());

// Logs → OTLP (plus Console)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry(o =>
{
    o.IncludeScopes = true;
    o.ParseStateValues = true;
    o.IncludeFormattedMessage = true;
    o.SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
        .AddAttributes(new[]
        {
            new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName),
            new KeyValuePair<string, object>("service.namespace", "iot-smart-home"),
            new KeyValuePair<string, object>("host.name", Environment.MachineName)
        }));
    o.AddOtlpExporter(); // usará OTEL_EXPORTER_OTLP_* do ambiente
});

builder.Services.AddHostedService<DatabaseWatchdogService>();

builder.Services
   .AddCore()
   .AddMemoryCache()
   .AddMySqlData(connectionString!);

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(b => b.Expire(TimeSpan.FromSeconds(2)));
});

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        });
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
var app = builder.Build();
// Enrich current span with useful request-scoped tags
app.Use(async (ctx, next) =>
{
    var activity = Activity.Current;
    if (activity != null)
    {
        var deviceId = ctx.Request.Headers["X-Device-Id"].ToString();
        var tenant = ctx.Request.Headers["X-Tenant"].ToString();
        if (!string.IsNullOrEmpty(deviceId)) activity.SetTag("device.id", deviceId);
        if (!string.IsNullOrEmpty(tenant)) activity.SetTag("tenant", tenant);
    }

    try
    {
        await next();
    }
    catch (Exception ex)
    {
        activity?.AddException(ex);
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        throw;
    }
});

app.UseCors("AllowAll");
app.UseOutputCache();
app.MapControllers();
app.UseMiddleware<ExceptionHandler>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/api/v1/timezone", (string zone) =>
{
    var response = new TimezoneResponse(zone);

    return Results.Ok(response);
})
.WithMetadata(new { Description = "Obtém informações de fuso horário" });


Console.CancelKeyPress += (sender, eventArgs) =>
{
    cts.Cancel();
    eventArgs.Cancel = true;
};

AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
{
    cts.Cancel();
};

await app.RunAsync(cts.Token);
