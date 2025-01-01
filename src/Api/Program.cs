using Api.Models;
using Core.Contracts.Repositories;
using Data.SqlServer.DI;

var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("Devices");
builder.Services.AddOpenApi();
builder.Services.AddSqlServerData(connectionString!);

var app = builder.Build();

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

app.MapGet("/api/v1/devices", async (IDeviceRepository repository) =>
{
    var devices = await repository.GetDevicesAsync();
    var models = devices.Select(Device.Create).ToArray();
    return Results.Ok(models);
});

await app.RunAsync();
