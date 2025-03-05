using GustaffWeb.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var connection = builder.Configuration.GetConnectionString("DbGust");

builder.Services.AddDbContext<Context>(options => options.UseMySql(connection, Microsoft.EntityFrameworkCore.ServerVersion.Parse("3.0.38-mysql")));

builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<Context>().AddDefaultTokenProviders();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "AspNetCore.Cookies";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    options.SlidingExpiration = true;
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 4; // Senha deve ter pelo menos 4 caracteres
    options.Password.RequireDigit = true; // Requer pelo menos um dígito
    options.Password.RequireLowercase = false; // Letras minúsculas são opcionais
    options.Password.RequireUppercase = false; // Letras maiúsculas são opcionais
    options.Password.RequireNonAlphanumeric = false; // Caracteres especiais são opcionais
    options.Password.RequiredUniqueChars = 1; // Requer pelo menos 1 caractere único
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Inicio}/{action=Index}/{id?}");

app.Run();
