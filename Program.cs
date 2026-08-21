using ExitInterviewSystem.Data;
using ExitInterviewSystem.Helpers;
using ExitInterviewSystem.Models;
using ExitInterviewSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ExitInterviewSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ActiveDirectoryService>();
            builder.Services.AddScoped<AuditService>();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;
                    options.User.RequireUniqueEmail = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/Login";
                // Session cookie only: expires when browser is closed (isPersistent: false on SignIn)
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                // Critical: no MaxAge / no persistent cookie → browser close forces re-login
                options.Cookie.MaxAge = null;
            });

            var app = builder.Build();

            // South African date/time culture: dd/MM/yyyy (not US MM/dd)
            var culture = new System.Globalization.CultureInfo("en-ZA");
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en-ZA"),
                SupportedCultures = new[] { culture },
                SupportedUICultures = new[] { culture }
            });

            // Create / repair database so Identity tables always exist
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();

                    var needsRecreate = true;
                    if (await context.Database.CanConnectAsync())
                    {
                        try
                        {
                            var conn = context.Database.GetDbConnection();
                            if (conn.State != System.Data.ConnectionState.Open)
                                await conn.OpenAsync();

                            using var cmd = conn.CreateCommand();
                            cmd.CommandText = @"
                                SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
                                WHERE TABLE_NAME = 'AspNetUsers'";
                            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                            cmd.CommandText = @"
                                SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES
                                WHERE TABLE_NAME = 'TerminationTypes'";
                            var termCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                            needsRecreate = (count == 0 || termCount == 0);
                            await conn.CloseAsync();
                        }
                        catch
                        {
                            needsRecreate = true;
                        }
                    }

                    if (needsRecreate)
                    {
                        logger.LogWarning("Identity tables missing — recreating database...");
                        await context.Database.EnsureDeletedAsync();
                        await context.Database.EnsureCreatedAsync();
                        logger.LogInformation("Database created successfully with Identity tables.");
                    }
                    else
                    {
                        await context.Database.EnsureCreatedAsync();
                    }

                    // Ensure AD profile columns exist on AspNetUsers
                    try
                    {
                        await context.Database.ExecuteSqlRawAsync(@"
                            IF COL_LENGTH('AspNetUsers', 'FirstName') IS NULL ALTER TABLE AspNetUsers ADD FirstName nvarchar(max) NULL;
                            IF COL_LENGTH('AspNetUsers', 'LastName') IS NULL ALTER TABLE AspNetUsers ADD LastName nvarchar(max) NULL;
                            IF COL_LENGTH('AspNetUsers', 'UserPrincipalName') IS NULL ALTER TABLE AspNetUsers ADD UserPrincipalName nvarchar(max) NULL;
                            IF COL_LENGTH('AspNetUsers', 'Company') IS NULL ALTER TABLE AspNetUsers ADD Company nvarchar(max) NULL;
                            IF COL_LENGTH('AspNetUsers', 'Telephone') IS NULL ALTER TABLE AspNetUsers ADD Telephone nvarchar(max) NULL;
                            IF COL_LENGTH('AspNetUsers', 'Mobile') IS NULL ALTER TABLE AspNetUsers ADD Mobile nvarchar(max) NULL;
                            IF COL_LENGTH('AspNetUsers', 'EmployeeId') IS NULL ALTER TABLE AspNetUsers ADD EmployeeId nvarchar(max) NULL;
                            IF COL_LENGTH('AspNetUsers', 'Description') IS NULL ALTER TABLE AspNetUsers ADD Description nvarchar(max) NULL;
                            IF COL_LENGTH('AspNetUsers', 'Manager') IS NULL ALTER TABLE AspNetUsers ADD Manager nvarchar(max) NULL;
                            IF COL_LENGTH('AspNetUsers', 'DistinguishedName') IS NULL ALTER TABLE AspNetUsers ADD DistinguishedName nvarchar(max) NULL;
                            IF COL_LENGTH('AspNetUsers', 'ExtensionAttribute1') IS NULL ALTER TABLE AspNetUsers ADD ExtensionAttribute1 nvarchar(max) NULL;
                            IF COL_LENGTH('AspNetUsers', 'EmployeeType') IS NULL ALTER TABLE AspNetUsers ADD EmployeeType nvarchar(max) NULL;
                            IF COL_LENGTH('AspNetUsers', 'AdGroups') IS NULL ALTER TABLE AspNetUsers ADD AdGroups nvarchar(max) NULL;
                        ");
                    }
                    catch (Exception colEx)
                    {
                        logger.LogWarning(colEx, "Could not ensure AD profile columns on AspNetUsers.");
                    }

                    // Ensure UserLevelPermissions table exists (EnsureCreated does not add new tables to existing DBs)
                    try
                    {
                        await context.Database.ExecuteSqlRawAsync(@"
                            IF OBJECT_ID(N'dbo.UserLevelPermissions', N'U') IS NULL
                            BEGIN
                                CREATE TABLE dbo.UserLevelPermissions (
                                    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                    RoleId nvarchar(128) NOT NULL,
                                    TableName nvarchar(100) NOT NULL,
                                    CanAdd bit NOT NULL DEFAULT 0,
                                    CanDelete bit NOT NULL DEFAULT 0,
                                    CanEdit bit NOT NULL DEFAULT 0,
                                    CanList bit NOT NULL DEFAULT 0,
                                    CanView bit NOT NULL DEFAULT 0,
                                    CanSearch bit NOT NULL DEFAULT 0
                                );
                                CREATE INDEX IX_UserLevelPermissions_RoleId ON dbo.UserLevelPermissions(RoleId);
                            END
                        ");
                    }
                    catch (Exception tblEx)
                    {
                        logger.LogWarning(tblEx, "Could not ensure UserLevelPermissions table.");
                    }

                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    string[] roles =
                    {
                        "Anonymous", "Administrator", "Default",
                        "IT System Support", "HR Institution", "HR Institution Manager",
                        "HR District Manager", "HR Head Office Manager", "Activator"
                    };
                    foreach (var role in roles)
                    {
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            await roleManager.CreateAsync(new IdentityRole(role));
                            logger.LogInformation("Created role: {Role}", role);
                        }
                    }

                    // No local admin seed — users appear only after AD login.
                    // Remove any previously seeded local "admin" account so Users shows only AD logins.
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var legacyAdmin = await userManager.FindByNameAsync("admin");
                    if (legacyAdmin != null)
                    {
                        await userManager.DeleteAsync(legacyAdmin);
                        logger.LogInformation("Removed legacy local admin user (AD-only policy).");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Database initialization failed. The app will still start; fix the connection string and restart.");
                }
            }

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

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
