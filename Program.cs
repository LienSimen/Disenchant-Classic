using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using WarDB.Models;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
Env.Load();

string connectionString = $"server={Env.GetString("DB_HOST")};" +
                          $"port={Env.GetString("DB_PORT")};" +
                          $"database={Env.GetString("DB_NAME")};" +
                          $"user={Env.GetString("DB_USER")};" +
                          $"password={Env.GetString("DB_PASSWORD")};";

// Register database context
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySQL(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
