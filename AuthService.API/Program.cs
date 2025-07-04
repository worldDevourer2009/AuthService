using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using AuthService.API.Middleware.Exceptions;
using AuthService.Application;
using AuthService.Application.Options;
using AuthService.Application.Services;
using AuthService.Domain.Services.Tokens;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(config => { config.RegisterServicesFromAssembly(typeof(Program).Assembly); });

// Bind options

builder.Services.AddNpgsql<AppDbContext>(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("AuthService.Infrastructure"));

builder.Services.AddHealthChecks()
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? string.Empty, name: "redis");

builder.Services
    .AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("JwtSettings"));

builder.Services
    .AddOptions<RedisSettings>()
    .Bind(builder.Configuration.GetSection("Redis"));

builder.Services
    .AddOptions<KafkaSettings>()
    .Bind(builder.Configuration.GetSection("Kafka"));

builder.Services
    .AddOptions<RsaKeySettings>()
    .Bind(builder.Configuration.GetSection("RsaKeySettings"));

builder.Services
    .AddOptions<InternalAuth>()
    .Bind(builder.Configuration.GetSection("InternalAuth"));

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80);
    options.ListenLocalhost(9500, options => { });
});

// Bind infrastructure layer
builder.Services.AddInfrastructure();

// Bind application layer
builder.Services.AddApplication();


// Bind rate limiter
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// Auth and Authorization

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("User", policy => policy.RequireRole("User"));
    options.AddPolicy("Anonymous", policy => policy.RequireRole("Anonymous"));
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
    
    options.AddPolicy("OnlyServices", policy =>
    {
        policy.AddAuthenticationSchemes("ServiceScheme");
        policy.RequireClaim("scope", "internal_api");
    });
});

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        using var scope = builder.Services.BuildServiceProvider().CreateScope();
        var keyGen = scope.ServiceProvider.GetRequiredService<IKeyGenerator>();
        var jwtSetting = scope.ServiceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(keyGen.Rsa),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = jwtSetting.Issuer,
            ValidAudience = jwtSetting.Audience
        };

        options.TokenValidationParameters.CryptoProviderFactory = new CryptoProviderFactory
        {
            CacheSignatureProviders = false
        };

        options.Events = new JwtBearerEvents()
        {
            OnTokenValidated = async context =>
            {
                var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
                var jwt = context.SecurityToken as JwtSecurityToken;

                if (jwt == null)
                {
                    context.Fail("Invalid token");
                    return;
                }

                if (await tokenService.IsAccessTokenRevokedForUser(jwt.RawData))
                {
                    context.Fail("Invalid token");
                }
            },
            OnAuthenticationFailed = context => { return Task.CompletedTask; }
        };
    })
    .AddJwtBearer("ServiceScheme", options =>
    {
        using var scope = builder.Services.BuildServiceProvider().CreateScope();
        var keyGen = scope.ServiceProvider.GetRequiredService<IKeyGenerator>();
        var internalAuthSettings = scope.ServiceProvider.GetRequiredService<IOptions<InternalAuth>>().Value;
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(keyGen.Rsa),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = internalAuthSettings.Issuer,
            ValidAudience = internalAuthSettings.Audience
        };
        
        options.TokenValidationParameters.CryptoProviderFactory = new CryptoProviderFactory
        {
            CacheSignatureProviders = false
        };
        
        options.Events = new JwtBearerEvents()
        {
            OnTokenValidated = async context =>
            {
                var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
                var jwt = context.SecurityToken as JwtSecurityToken;

                if (jwt == null)
                {
                    context.Fail("Invalid token");
                    return;
                }

                if (await tokenService.IsAccessTokenRevokedForUser(jwt.RawData))
                {
                    context.Fail("Invalid token");
                }
            },
            OnAuthenticationFailed = context => { return Task.CompletedTask; }
        };
    });

//Exceptions
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

//Controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

//Logging
builder.Services.AddLogging();

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();

        var dataSeeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();

        await dataSeeder.SeedDataAsync();
    }
}

app.UseHealthChecks("/health");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;