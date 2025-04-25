
using ControlMDBI.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using static ControlMDBI.Controllers.AuthController;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ControlMDBIDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ControlDBConex"),
        new MySqlServerVersion(new Version(8, 0, 41)) // Especifica tu versi�n de MySQL

    ));


builder.Services.AddSession();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Auth/Login";
        option.AccessDeniedPath = "/Home/Privacy";
    });
//builder.Services.AddScoped<ActiveUserFilter>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else // En Desarrollo muestra errores en la p�gina con detalles t�cnicos
{
    app.UseDeveloperExceptionPage();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ControlMDBIDbContext>();
    context.Database.EnsureCreated(); // Crea la base de datos si no existe
    DbInitializer.Inialize(context); // Inicializa la base de datos con datos de prueba
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Manejo de las �reas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
