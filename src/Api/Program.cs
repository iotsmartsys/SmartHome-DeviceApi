using Api.Models;
using Core.DI;
using Data.Repositories.MySql.DI;
using Microsoft.AspNetCore.OutputCaching;

var cts = new CancellationTokenSource();

var builder = WebApplication.CreateBuilder(args);
string? connectionString = builder.Configuration.GetConnectionString("Devices");
builder.Services.AddOpenApi();

builder.Services.AddHostedService<DatabaseWatchdogService>();

builder.Services
   .AddCore()
   .AddMemoryCache()
   .AddMySqlData(connectionString!);

builder.Services.AddOutputCache(options =>
{
    // Default policy is fine; per-endpoint attributes will set TTL
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

app.UseCors("AllowAll");
app.UseOutputCache();
app.MapControllers();
app.UseMiddleware<ExceptionHandler>();

// Configure the HTTP request pipeline.
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

app.MapGet("/api/v1/timezone/datetime", () =>
{
    return Results.Ok(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss"));
})
.WithMetadata(new { Description = "Obtém informações de data e hora para um fuso horário específico" });

app.MapGet("/api/v1/health", () => Results.Ok(new { status = "Healthy" }))
   .WithMetadata(new { Description = "Verifica o status de saúde da API" });

Console.CancelKeyPress += (sender, eventArgs) =>
{
    cts.Cancel();
    eventArgs.Cancel = true; // Permite shutdown gracioso
};

AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
{
    cts.Cancel();
};

await app.RunAsync(cts.Token);
