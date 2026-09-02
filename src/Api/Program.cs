using Api.Models;
using Core.DI;
using Data.Repositories.MySql.DI;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

var cts = new CancellationTokenSource();

var builder = WebApplication.CreateBuilder(args);
string? connectionString = builder.Configuration.GetConnectionString("Devices");
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName?.Replace('+', '.') ?? type.Name);
    options.MapType<JsonPatchDocument<GroupPatchRequest>>(() => new OpenApiSchema
    {
        Type = "array",
        Description = "Operações JSON Patch replace para /name, /active ou /icon.",
        Items = new OpenApiSchema
        {
            Type = "object",
            AdditionalPropertiesAllowed = false,
            Required = new HashSet<string> { "op", "path", "value" },
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["op"] = new()
                {
                    Type = "string",
                    Enum = new List<IOpenApiAny> { new OpenApiString("replace") }
                },
                ["path"] = new()
                {
                    Type = "string",
                    Enum = new List<IOpenApiAny>
                    {
                        new OpenApiString("/name"),
                        new OpenApiString("/active"),
                        new OpenApiString("/icon")
                    }
                },
                ["value"] = new()
                {
                    Nullable = true,
                    OneOf = new List<OpenApiSchema>
                    {
                        new() { Type = "string" },
                        new() { Type = "boolean" },
                        new()
                        {
                            Type = "object",
                            AdditionalPropertiesAllowed = false,
                            Required = new HashSet<string> { "name" },
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["name"] = new() { Type = "string" }
                            }
                        }
                    }
                }
            }
        }
    });
});

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
    app.UseSwagger();
    app.UseSwaggerUI();
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
