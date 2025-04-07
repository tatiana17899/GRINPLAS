using Microsoft.AspNetCore.Identity;
using GRINPLAS.Models;
using Microsoft.EntityFrameworkCore;
using GRINPLAS.Data;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("PostgreSQLConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var signInManager = services.GetRequiredService<SignInManager<ApplicationUser>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        if (!await roleManager.RoleExistsAsync("GerenteGeneral"))
        {
            await roleManager.CreateAsync(new IdentityRole("GerenteGeneral"));
        }

        if (!await roleManager.RoleExistsAsync("Administrador"))
        {
            await roleManager.CreateAsync(new IdentityRole("Administrador"));
        }

        var adminEmail = "nicanorguevara332@gmail.com";
        var adminPassword = "Tatiana123%&"; 
        var gerenteEmail = "mayraguevara332@gmail.com";
        var gerentePassword = "Tatiana132%&"; 

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser 
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true 
            };
            
            var createAdminResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createAdminResult.Succeeded)
            {
                throw new Exception($"Error al crear usuario administrador: {string.Join(", ", createAdminResult.Errors)}");
            }
            
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Administrador"))
        {
            await userManager.AddToRoleAsync(adminUser, "Administrador");
        }

        var gerenteUser = await userManager.FindByEmailAsync(gerenteEmail);
        if (gerenteUser == null)
        {
            gerenteUser = new ApplicationUser 
            {
                UserName = gerenteEmail,
                Email = gerenteEmail,
                EmailConfirmed = true 
            };
            
            var createGerenteResult = await userManager.CreateAsync(gerenteUser, gerentePassword);
            if (!createGerenteResult.Succeeded)
            {
                throw new Exception($"Error al crear usuario gerente: {string.Join(", ", createGerenteResult.Errors)}");
            }
            
        }

        if (!await userManager.IsInRoleAsync(gerenteUser, "GerenteGeneral"))
        {
            await userManager.AddToRoleAsync(gerenteUser, "GerenteGeneral");
        }

        await context.SaveChangesAsync();

        if (adminUser != null)
            await signInManager.RefreshSignInAsync(adminUser);
        if (gerenteUser != null)
            await signInManager.RefreshSignInAsync(gerenteUser);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar roles y usuarios.");
    }
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
