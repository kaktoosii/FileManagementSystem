using System.Text;
using Base.Common;
using Base.Common.Features.Identity;
using Base.Common.Helpers;
using Base.Common.SiteOptions;
using Base.DataLayer.Context;
using Base.DomainClasses;
using Base.Services;
using Base.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Services;
using Services.Contracts;
using Services.Services;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Base.IoCConfig;

public static class ConfigureServicesExtensions
{
    public static void AddCustomAntiforgery(this IServiceCollection services)
    {
        services.AddAntiforgery(x => x.HeaderName = "X-XSRF-TOKEN");
        services.AddMvc(options => { options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()); });
    }

    public static void AddCustomCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                    .WithOrigins(
                        "http://localhost:4200", "http://localhost:4300") //Note:  The URL must be specified without a trailing slash (/).
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(host => true)
                    .AllowCredentials());
        });
    }

    public static void AddCustomJwtBearer(this IServiceCollection services, IConfiguration configuration)
    {
        // Only needed for custom roles.
        services.AddAuthorization(options =>
        {
            options.AddPolicy(CustomRoles.Admin, policy => policy.RequireRole(CustomRoles.Admin));
            options.AddPolicy(CustomRoles.User, policy => policy.RequireRole(CustomRoles.User));
        });

        // Needed for jwt auth.
        services.AddAuthentication(options =>
        {
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Set cookie sign-in scheme
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(cfg =>
        {
            cfg.RequireHttpsMetadata = false; // Set to true for production
            cfg.SaveToken = true;

            var bearerTokenOption = configuration.GetSection("BearerTokens").Get<BearerTokensOptions>()
                ?? throw new InvalidOperationException("bearerTokenOption is null");

            cfg.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = bearerTokenOption.Issuer,
                ValidateIssuer = true,

                ValidAudience = bearerTokenOption.Audience,
                ValidateAudience = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(bearerTokenOption.Key)),

                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Token expiration tolerance
            };

            cfg.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                        .CreateLogger(nameof(JwtBearerEvents));

                    logger.LogError(context.Exception, "Authentication failed");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    var tokenValidatorService = context.HttpContext.RequestServices.GetRequiredService<ITokenValidatorService>();
                    return tokenValidatorService.ValidateAsync(context);
                },
                OnMessageReceived = context => Task.CompletedTask,
                OnChallenge = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                        .CreateLogger(nameof(JwtBearerEvents));

                    logger.LogError(context.Error, "OnChallenge error {Error}", context.ErrorDescription);
                    return Task.CompletedTask;
                }
            };
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = "auth_token"; // Cookie name
            options.LoginPath = "/auth/Loginwithcode"; // Path to redirect on unauthenticated access
            options.AccessDeniedPath = "/Account/AccessDenied"; // Path for unauthorized access
            options.SlidingExpiration = false;
            options.ExpireTimeSpan = TimeSpan.FromDays(7); // Cookie expiration time
            options.Cookie.HttpOnly = true; // Prevent access to the cookie via JavaScript
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Use HTTPS in production
            options.Cookie.SameSite = SameSiteMode.Lax; // Helps mitigate CSRF attacks
        });

    }

    public static void AddCustomDbContext(this IServiceCollection services, IConfiguration configuration,
        Assembly startupAssembly)
    {
        if (configuration == null)
        {
            throw new AppException(nameof(configuration));
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                serverDbContextOptionsBuilder =>
                {
                    var minutes = (int)TimeSpan.FromMinutes(3).TotalSeconds;
                    serverDbContextOptionsBuilder.CommandTimeout(minutes);
                    serverDbContextOptionsBuilder.EnableRetryOnFailure();
                });
        });
    }

    public static void AddCustomServices(this IServiceCollection services)
    {

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IApiActionsDiscoveryService, ApiActionsDiscoveryService>();
        services.AddScoped<IDeviceDetectionService, DeviceDetectionService>();
        services.AddScoped<IAntiForgeryCookieService, AntiForgeryCookieService>();
        services.AddScoped<IUnitOfWork, ApplicationDbContext>();
        services.AddScoped<IUsersService, UsersService>();
        services.AddScoped<IRolesService, RolesService>();
        services.AddSingleton<ISecurityService, SecurityService>();
        services.AddScoped<IDbInitializerService, DbInitializerService>();
        services.AddScoped<ITokenStoreService, TokenStoreService>();
        services.AddScoped<ITokenValidatorService, TokenValidatorService>();
        services.AddScoped<ITokenFactoryService, TokenFactoryService>();
        services.AddScoped<IUserClaimsService, UserClaimsService>();

        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<ISmsSender, MessageSender>();
        services.AddScoped<IContentGroupService, ContentGroupService>();
        services.AddScoped<IContentService, ContentService>();
        services.AddScoped<ISettingService, SettingService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<ISupportService, SupportService>();
        services.AddScoped<IReportService, ReportService>();

        services.AddDynamicPermissions();
        services.AddControllers().AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
    }
    private static IServiceCollection AddDynamicPermissions(this IServiceCollection services)
    {
        services.AddScoped<IServerSecurityTrimmingService, ServerSecurityTrimmingService>();
        services.AddScoped<IAuthorizationHandler, DynamicServerPermissionsAuthorizationHandler>();
        services.AddAuthorization(opts =>
        {
            opts.AddPolicy(
                CustomPolicies.DynamicServerPermission,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new DynamicServerPermissionRequirement());
                });
        });

        return services;
    }
    public static void AddCustomOptions(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration == null)
        {
            throw new AppException(nameof(configuration));
        }
        services.Configure<SiteSettingsDto>(configuration.Bind);
        services.AddOptions<BearerTokensOptions>()
            .Bind(configuration.GetSection("BearerTokens"))
            .Validate(
                bearerTokens =>
                {
                    return bearerTokens.AccessTokenExpirationMinutes < bearerTokens.RefreshTokenExpirationMinutes;
                },
                "RefreshTokenExpirationMinutes is less than AccessTokenExpirationMinutes. Obtaining new tokens using the refresh token should happen only if the access token has expired.");

        services.AddOptions<ApiSettings>().Bind(configuration.GetSection(key: "ApiSettings"));
        services.AddOptions<AdminUserSeed>().Bind(configuration.GetSection(key: "AdminUserSeed"));
    }

    public static void UseCustomSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(setupAction =>
        {
            setupAction.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                "WaterMeter API V1");
            setupAction.SwaggerEndpoint(
                 "/swagger/v2/swagger.json",
                 "WaterMeter API V2");
            //setupAction.RoutePrefix = ""; --> To be able to access it from this URL: https://localhost:5001/swagger/index.html

            setupAction.DefaultModelExpandDepth(2);
            setupAction.DefaultModelRendering(ModelRendering.Model);
            setupAction.DocExpansion(DocExpansion.None);
            setupAction.EnableDeepLinking();
            setupAction.DisplayOperationId();
        });
    }

    public static void AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(setupAction =>
        {
            setupAction.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "WaterMeter API",
                    Version = "v1",
                    Description = "Through this API you can access the site's capabilities.",
                    Contact = new OpenApiContact
                    {
                        Email = "info@znrw.ir",
                        Name = "WaterMeter",
                        Url = new Uri("https://api.Shop.ir/")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                }
                );
            setupAction.SwaggerDoc(
                     "v2",
                     new OpenApiInfo
                     {
                         Title = "WaterMeter Roles",
                         Description = "Through this API you can access the site's capabilities.",
                         Version = "v1",
                         Contact = new OpenApiContact
                         {
                             Email = "info@znrw.ir",
                             Name = "WaterMeter",
                             Url = new Uri("https://api.Shop.ir/")
                         },
                         License = new OpenApiLicense
                         {
                             Name = "MIT License",
                             Url = new Uri("https://opensource.org/licenses/MIT")
                         }
                     }
                 );
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly)
                .ToList();
            xmlFiles.ForEach(xmlFile => setupAction.IncludeXmlComments(xmlFile));

            setupAction.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            setupAction.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new List<string>()
                }
            });
        });
    }
}