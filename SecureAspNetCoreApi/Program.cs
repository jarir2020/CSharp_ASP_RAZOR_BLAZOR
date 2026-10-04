using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SecureAspNetCoreApi.Models;
using SecureAspNetCoreApi.Security;

namespace SecureAspNetCoreApi;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        JwtOptions jwtOptions = JwtOptions.FromConfiguration(builder.Configuration);

        builder.Services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("AuthDb")
                ?? "Data Source=secure-api.db"));

        builder.Services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddSingleton(jwtOptions);
        builder.Services.AddScoped<JwtTokenService>();

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("CourseManagement", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("permission", "course.manage"));
        });

        string allowedOrigin = builder.Configuration["Security:AllowedOrigin"]
            ?? "http://localhost:3000";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy => policy
                .WithOrigins(allowedOrigin)
                .AllowAnyHeader()
                .AllowAnyMethod());
        });

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddFixedWindowLimiter("auth", limiterOptions =>
            {
                // Keep integration tests independent while retaining the
                // production policy of five authentication requests per minute.
                limiterOptions.PermitLimit = builder.Environment.IsEnvironment("Testing") ? 100 : 5;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueLimit = 0;
            });
        });

        builder.Services.AddControllers();
        WebApplication app = builder.Build();

        await InitializeDatabaseAsync(app.Services);

        if (!app.Environment.IsEnvironment("Testing"))
        {
            // Production should serve the API through HTTPS. Tests use the
            // in-memory server without a TLS endpoint.
            app.UseHttpsRedirection();
        }

        app.Use(async (context, next) =>
        {
            // These headers reduce common browser interpretation risks. JSON
            // serializers also encode string values rather than rendering HTML.
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
            await next();
        });

        app.UseCors("Frontend");
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        await app.RunAsync();
    }

    private static async Task InitializeDatabaseAsync(IServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        AuthDbContext db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await db.Database.EnsureCreatedAsync();
        await AuthSeeder.SeedRolesAsync(scope.ServiceProvider);
    }
}
