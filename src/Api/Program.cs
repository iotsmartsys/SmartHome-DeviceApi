using Api.Models;
using Core.DI;
using Data.Repositories.MySql.DI;

var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("Devices");
builder.Services.AddOpenApi();
builder.Services
    .AddCore()
    .AddMemoryCache()
    .AddMySqlData(connectionString!)
    .AddRabbitMq(builder.Configuration);
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
