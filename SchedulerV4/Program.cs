using System.IO;
using Microsoft.EntityFrameworkCore;
using SchedulerV4.Models;

var builder = WebApplication.CreateBuilder(args);

// Путь к базе внутри проекта — "Data/FIRST_DB.FDB"
var dbFilePath = Path.Combine(builder.Environment.ContentRootPath, "Data", "FIRST_DB.FDB");

// Формируем корректную строку подключения для Firebird
var connectionString = $"Server=localhost;Database={dbFilePath};User=SYSDBA;Password=masterkey;";

builder.Services.AddControllersWithViews();

// Добавляем DbContext с созданной строкой подключения
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseFirebird(connectionString),
    ServiceLifetime.Scoped
);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
