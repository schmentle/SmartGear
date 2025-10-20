using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SmartGear.PM0902.Data;
using SmartGear.PM0902.Filters;
using SmartGear.PM0902.Middleware;
using SmartGear.PM0902.Models;
using SmartGear.PM0902.Repositories;
using SmartGear.PM0902.Services;

var builder = WebApplication.CreateBuilder(args);

// MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// App services
builder.Services.AddScoped<LogActionFilter>();
builder.Services.AddScoped<AdminHeaderAuthorizeFilter>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRequestTimeService, RequestTimeService>();
builder.Services.AddTransient<RequestLoggingMiddleware>();

// EF Core
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var csSqlite = builder.Configuration.GetConnectionString("Sqlite");
    if (!string.IsNullOrWhiteSpace(csSqlite) && !builder.Environment.IsDevelopment())
    {
        opt.UseSqlite(csSqlite);
    }
    else
    {
        opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")); 
    }
});

// Identity WITH roles (single registration)
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(opts =>
    {
        opts.SignIn.RequireConfirmedAccount = false;
        opts.Password.RequiredLength = 6;
        opts.Password.RequireNonAlphanumeric = false;
        opts.Password.RequireUppercase = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultUI(); 

// Anti-forgery
builder.Services.AddAntiforgery(o =>
{
    o.HeaderName = "X-CSRF-TOKEN";
});

// Repositories
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ProductRepository>(); 
builder.Services.AddScoped<IProductRepository>(sp =>
    new ProductRepositoryCached(
        sp.GetRequiredService<ProductRepository>(),
        sp.GetRequiredService<IMemoryCache>()));
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("RequireSalesRep", p => p.RequireRole("SalesRep"));
});

builder.Services.AddSignalR();

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapRazorPages();     
app.MapControllers();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/__endpoints", (EndpointDataSource es) =>
    string.Join("\n", es.Endpoints
        .OfType<RouteEndpoint>()
        .Select(e => $"{e.DisplayName}  =>  {e.RoutePattern.RawText}")));

app.MapGet("/ping", () => Results.Ok("pong"));

app.MapHub<SmartGear.PM0902.Hubs.ProductHub>("/hubs/products");

app.MapGet("/whoami", (HttpContext ctx) => new {
    Authenticated = ctx.User.Identity?.IsAuthenticated,
    Name = ctx.User.Identity?.Name,
    Roles = ctx.User.Claims.Where(c => c.Type.EndsWith("/role") || c.Type == "role")
                           .Select(c => c.Value)
});

// Migrate & seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await SeedIdentity(scope.ServiceProvider);
}

app.Run();

static async Task SeedIdentity(IServiceProvider sp)
{
    var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = ["Admin", "SalesRep", "Customer"];
    foreach (var r in roles)
        if (!await roleMgr.RoleExistsAsync(r))
            await roleMgr.CreateAsync(new IdentityRole(r));

    var adminEmail = "admin@smartgear.local";
    var admin = await userMgr.FindByEmailAsync(adminEmail);
    if (admin is null)
    {
        admin = new ApplicationUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        await userMgr.CreateAsync(admin, "Admin#123");
    }

    if (!await userMgr.IsInRoleAsync(admin, "Admin"))
        await userMgr.AddToRoleAsync(admin, "Admin");

}