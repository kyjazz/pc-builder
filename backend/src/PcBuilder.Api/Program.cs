using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PcBuilder.Api.Data;
using PcBuilder.Api.Data.Seed;
using PcBuilder.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Locally this comes from appsettings.Development.json (Windows auth, no secret).
// On Azure it comes from the ConnectionStrings__DefaultConnection environment variable.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sql =>
        // Local SQLEXPRESS is SQL Server 2017 (level 140). Pinning it stops EF from
        // generating T-SQL that only newer versions understand.
        sql.UseCompatibilityLevel(140)));

builder.Services.AddScoped<IPartService, PartService>();

builder.Services.AddControllers()
    // Enums travel as text in JSON ("Gpu", "DDR5"), matching the mobile app's TypeScript types.
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Every error response (404, 400, unhandled exception) uses the ProblemDetails format.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Fill an empty database with the starter parts catalogue.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
    var seedFile = Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "parts.json");
    await DbSeeder.SeedPartsAsync(db, seedFile, logger);
}

// Unhandled exceptions -> 500 ProblemDetails (no stack trace leaks to clients).
app.UseExceptionHandler();
// Bodiless error codes (e.g. 404 for an unknown route) -> ProblemDetails.
app.UseStatusCodePages();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
