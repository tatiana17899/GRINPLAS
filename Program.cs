using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GRINPLAS.Data;
using GRINPLAS.Models;
using Rotativa.AspNetCore;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(3);
});

var connectionString = builder.Configuration.GetConnectionString("PostgreSQLConnection") ?? throw new InvalidOperationException("Connection string not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
    
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Configuración del servicio de correo
var emailConfig = builder.Configuration.GetSection("EmailSettings");
builder.Services.AddTransient<IEmailSender>(provider => 
    new EmailSender(
        emailConfig["SmtpServer"],
        int.Parse(emailConfig["SmtpPort"]),
        emailConfig["FromEmail"],
        emailConfig["SmtpUsername"],
        emailConfig["SmtpPassword"]
    ));
    
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        var roles = new[] { "GerenteGeneral", "Administrador", "Vendedor"};
        
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
        var clienteEmail = "cliente123@gmail.com";
        var clientePassword = "Tatiana321%&";
        var adminEmail = "nicanorguevara332@gmail.com";
        var adminPassword = "Tatiana123%&"; 
        await EnsureUserAsync(userManager, adminEmail, adminPassword, "Administrador");

        var gerenteEmail = "mayraguevara332@gmail.com";
        var gerentePassword = "Tatiana132%&"; 
        await EnsureUserAsync(userManager, gerenteEmail, gerentePassword, "GerenteGeneral");

        await context.SaveChangesAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar roles y usuarios.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
RotativaConfiguration.Setup(app.Environment.WebRootPath);
// Luego usa Rotativa
app.UseRotativa();

app.Use(async (context, next) =>
{
    if (!context.User.Identity.IsAuthenticated && context.Request.Path == "/")
    {
        context.Response.Redirect("/Home/Index");
        return;
    }
    await next();
});


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Productos}/{action=Cliente}/{id?}");
app.MapRazorPages();

app.Run();

async Task EnsureUserAsync(UserManager<ApplicationUser> userManager, string email, string password, string role)
{
    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
    {
        user = new ApplicationUser 
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true 
        };
        
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new Exception($"Error al crear usuario {email}: {string.Join(", ", result.Errors)}");
        }
    }

    if (!await userManager.IsInRoleAsync(user, role))
    {
        await userManager.AddToRoleAsync(user, role);
    }
}