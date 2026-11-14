using Microsoft.EntityFrameworkCore;
using PcBuilder.Api.Data;
using PcBuilder.Api.Data.Seed;

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

builder.Services.AddControllers();
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
