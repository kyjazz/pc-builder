using Microsoft.EntityFrameworkCore;
using PcBuilder.Api.Data;

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
