using Api.Models;
using Data.Repositories.MySql.DI;
using Core.DI;

var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("Devices");
builder.Services.AddOpenApi();
builder.Services
    .AddCore()
    // .AddSqlServerData(connectionString!)
    .AddMySqlData(connectionString!)
    .AddRabbitMq(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

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


await app.RunAsync();
