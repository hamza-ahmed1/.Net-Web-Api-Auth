using Auth.Data;
using Auth.Model.Entities;
using Auth.Services;
using Auth.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});


builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ISectionService, SectionService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ITeacherSectionCourseService, TeacherSectionCourseService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IExamTypeService,ExamTypeService>();
builder.Services.AddScoped<IExamResultService, ExamResultService>();
builder.Services.AddScoped<IFeeCategoryService, FeeCategoryService>();
builder.Services.AddScoped<IFeeTypeService, FeeTypeService>();
builder.Services.AddScoped<IApplicableFeeService, ApplicableFeeService>();




builder.Services.AddCors(options =>
{
    options.AddPolicy("SpaPolicy", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {

        o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        o.JsonSerializerOptions.MaxDepth = 64;
    });

var app = builder.Build();

app.UseCors("SpaPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>()
        .CreateLogger("Migration");

    // Try to apply pending EF Core migrations with a retry loop so the container
    // can start even if the database isn't immediately available when the
    // API container launches (common in docker-compose environments).
    const int maxAttempts = 12;
    const int delaySeconds = 5;
    for (int attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            db.Database.Migrate();
            logger.LogInformation("Database migration applied successfully.");
            break;
        }
        catch (Exception ex)
        {
            // If migration failed due to attempting to change an IDENTITY property,
            // abort automatic migrations and require a manual migration that drops
            // and recreates the affected column(s).
            if (ex is InvalidOperationException && ex.Message != null && ex.Message.Contains("To change the IDENTITY property of a column"))
            {
                logger.LogError(ex, "Migration aborted: changing IDENTITY properties requires dropping and recreating the column. Please apply migration manually.");
                break;
            }

            logger.LogWarning(ex, "Database migration attempt {Attempt}/{Max} failed.", attempt, maxAttempts);
            if (attempt == maxAttempts)
            {
                logger.LogError(ex, "Max migration attempts reached, rethrowing.");
                throw;
            }
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }
    }
    // Seed default data (roles, admin user). Values can be configured via appsettings or env variables:
    // Admin:UserName, Admin:Password, Admin:Role, Admin:Seed (true/false)
    try
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var config = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();

        var seedAdmin = true;
        var seedCfg = config["Admin:Seed"] ?? Environment.GetEnvironmentVariable("ADMIN_SEED");
        if (!string.IsNullOrWhiteSpace(seedCfg) && bool.TryParse(seedCfg, out var parsed)) seedAdmin = parsed;

        if (!seedAdmin)
        {
            logger.LogInformation("Admin seeding skipped by configuration (Admin:Seed=false).");
        }
        else
        {
            var adminRole = config["Admin:Role"] ?? Environment.GetEnvironmentVariable("ADMIN_ROLE") ?? "Admin";
            var adminUserName = config["Admin:UserName"] ?? Environment.GetEnvironmentVariable("ADMIN_USERNAME") ?? "lms.admin@gmail.com";
            var adminPassword = config["Admin:Password"] ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "Admin@1234";

            // ensure role exists
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                var r = new IdentityRole(adminRole);
                var rr = await roleManager.CreateAsync(r);
                if (rr.Succeeded)
                    logger.LogInformation("Created role '{Role}'", adminRole);
                else
                    logger.LogWarning("Failed creating role '{Role}': {Errors}", adminRole, string.Join(';', rr.Errors.Select(e => e.Description)));
            }

            // Look up by email first, then by username
            var existing = await userManager.FindByEmailAsync(adminUserName) ?? await userManager.FindByNameAsync(adminUserName);
            if (existing == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminUserName,
                    EmailConfirmed = true,
                    FullName = "Administrator"
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    logger.LogInformation("Created admin user '{UserName}'", adminUserName);
                    var roleAdd = await userManager.AddToRoleAsync(admin, adminRole);
                    if (roleAdd.Succeeded)
                        logger.LogInformation("Assigned '{UserName}' to role '{Role}'", adminUserName, adminRole);
                    else
                        logger.LogWarning("Failed to assign role '{Role}' to '{UserName}': {Errors}", adminRole, adminUserName, string.Join(';', roleAdd.Errors.Select(e => e.Description)));
                }
                else
                {
                    logger.LogWarning("Failed creating admin user '{UserName}': {Errors}", adminUserName, string.Join(';', result.Errors.Select(e => e.Description)));
                    foreach (var err in result.Errors)
                        logger.LogDebug("Identity error: {Code} - {Desc}", err.Code, err.Description);
                }
            }
            else
            {
                // Ensure admin is in role
                if (!await userManager.IsInRoleAsync(existing, adminRole))
                {
                    var addRes = await userManager.AddToRoleAsync(existing, adminRole);
                    if (addRes.Succeeded)
                        logger.LogInformation("Added existing user '{UserName}' to role '{Role}'", adminUserName, adminRole);
                    else
                        logger.LogWarning("Failed to add existing user '{UserName}' to role '{Role}': {Errors}", adminUserName, adminRole, string.Join(';', addRes.Errors.Select(e => e.Description)));
                }
                else
                {
                    logger.LogInformation("Admin user '{UserName}' already exists and is in role '{Role}'", adminUserName, adminRole);
                }
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding default data.");
    }
    // Run additional domain seeding (teachers, students, sections, courses)
    try
    {
        await SeedDate.InitializeAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while running SeedDate.InitializeAsync.");
    }
}
app.Run();