using KhaoSat.Middleware;
using KhaoSat.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// 1. Add MVC
builder.Services.AddControllersWithViews();

// 2. Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


var app = builder.Build();

// 4. Error & HSTS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 5. Standard middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// 6. Custom middleware
app.UseMiddleware<RoleRedirectMiddleware>();

app.UseAuthorization();

// 7. Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.Run();
