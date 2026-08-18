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


builder.Services.AddScoped<Auth.Services.ITokenService, TokenService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ISectionService, SectionService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ITeacherSectionCourseService, TeacherSectionCourseService>();
builder.Services.AddScoped<IStudentService, StudentService>();
//builder.Services.AddScoped<IStudentEnrollmentService, StudentEnrollmentService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();



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
            logger.LogWarning(ex, "Database migration attempt {Attempt}/{Max} failed.", attempt, maxAttempts);
            if (attempt == maxAttempts)
            {
                logger.LogError(ex, "Max migration attempts reached, rethrowing.");
                throw;
            }
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }
    }
    // Seed default data (roles, admin user)
    try
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        const string adminRole = "Admin";
        const string adminUserName = "lms.admin.com";
        const string adminPassword = "Admin@1234";

        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            var r = new IdentityRole(adminRole);
            var rr = await roleManager.CreateAsync(r);
            if (rr.Succeeded)
                logger.LogInformation("Created role '{Role}'", adminRole);
            else
                logger.LogWarning("Failed creating role '{Role}': {Errors}", adminRole, string.Join(';', rr.Errors.Select(e => e.Description)));
        }

        var existing = await userManager.FindByNameAsync(adminUserName);
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
                await userManager.AddToRoleAsync(admin, adminRole);
                logger.LogInformation("Assigned '{UserName}' to role '{Role}'", adminUserName, adminRole);
            }
            else
            {
                logger.LogWarning("Failed creating admin user '{UserName}': {Errors}", adminUserName, string.Join(';', result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // Ensure admin is in role
            if (!await userManager.IsInRoleAsync(existing, adminRole))
            {
                await userManager.AddToRoleAsync(existing, adminRole);
                logger.LogInformation("Added existing user '{UserName}' to role '{Role}'", adminUserName, adminRole);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding default data.");
    }
}
app.Run();