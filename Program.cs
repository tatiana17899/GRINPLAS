using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GRINPLAS.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using GRINPLAS.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("PostgreSQLConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Crear roles si no existen
        if (!await roleManager.RoleExistsAsync("GerenteGeneral"))
        {
            await roleManager.CreateAsync(new IdentityRole("GerenteGeneral"));
        }

        if (!await roleManager.RoleExistsAsync("Administrador"))
        {
            await roleManager.CreateAsync(new IdentityRole("Administrador"));
        }

        // Crear usuario administrador
        var adminEmail = "nicanorguevara332@gmail.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser 
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true 
            };
            
            var createAdminResult = await userManager.CreateAsync(adminUser, "Tatiana123%&");
            if (createAdminResult.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrador");
            }
        }

        // Crear usuario gerente
        var gerenteEmail = "mayraguevara332@gmail.com";
        var gerenteUser = await userManager.FindByEmailAsync(gerenteEmail);
        if (gerenteUser == null)
        {
            gerenteUser = new ApplicationUser 
            {
                UserName = gerenteEmail,
                Email = gerenteEmail,
                EmailConfirmed = true 
            };
            
            var createGerenteResult = await userManager.CreateAsync(gerenteUser, "Tatiana132%&");
            if (createGerenteResult.Succeeded)
            {
                await userManager.AddToRoleAsync(gerenteUser, "GerenteGeneral");
            }
        }

        await context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar roles y usuarios.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
