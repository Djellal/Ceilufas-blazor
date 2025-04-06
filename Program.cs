using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ceilufas.Components;
using Ceilufas.Components.Account;
using Ceilufas.Data;
using Ceilufas.Services;  // Add this using statement

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register the Globals service as a singleton
builder.Services.AddSingleton<Globals>();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = true;
    
    // Configure password requirements
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

// Apply migrations and seed roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Apply migrations
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        
        // Seed roles
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = new[] { Globals.Admin, Globals.Teacher, Globals.Student };
        
        foreach (var role in roles)
        {
            if (!roleManager.RoleExistsAsync(role).Result)
            {
                roleManager.CreateAsync(new IdentityRole(role)).Wait();
            }
        }
        
        // Seed admin user
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var adminEmail = "djellal@univ-setif.dz";
        var adminUser =await  userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            // Create the admin user if it doesn't exist
            
            adminUser = new ApplicationUser
            {
                UserName = "djellal",
                Email = adminEmail,
                EmailConfirmed = true // Skip email confirmation
            };
            
            var result = await userManager.CreateAsync(adminUser, "dhb571982");
            if (result.Succeeded)
            {
                userManager.AddToRoleAsync(adminUser, Globals.Admin).Wait();
                Console.WriteLine("Admin user created successfully.");
            }else
            {
                Console.WriteLine("Failed to create admin user.");
            }
        }
        else if (!userManager.IsInRoleAsync(adminUser, Globals.Admin).Result)
        {
            // Ensure the user is in the Admin role if they already exist
            userManager.AddToRoleAsync(adminUser, Globals.Admin).Wait();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        Console.WriteLine(ex.Message);
    }
}

app.Run();
