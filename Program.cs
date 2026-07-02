using EBook.Model;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext
builder.Services.AddDbContext<Data_Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("project"),
        sqlOptions => sqlOptions.CommandTimeout(60)
    ));

builder.Services.AddHttpContextAccessor();

// Add MVC services
builder.Services.AddControllersWithViews();

// ? Enable session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseRouting();

// ? Session middleware must come before Authorization
app.UseSession();

app.UseAuthorization();

// Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Home}/{id?}");

app.Run();
