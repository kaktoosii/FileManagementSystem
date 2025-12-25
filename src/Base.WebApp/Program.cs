using Base.Common.Helpers;
using Base.IoCConfig;
using Base.Services;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Localization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
ConfigureLogging(builder.Logging, builder.Environment, builder.Configuration);
ConfigureServices(builder.Services, builder.Configuration);
var webApp = builder.Build();
// Use localization middleware
var supportedCultures = new[] { "en-US", "fa-IR" };
var cultures = supportedCultures.Select(culture => new CultureInfo(culture)).ToList();

// Use localization middleware
webApp.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("fa-IR"),
    SupportedCultures = cultures,
    SupportedUICultures = cultures
});
ConfigureMiddlewares(webApp, webApp.Environment);
ConfigureEndpoints(webApp, webApp.Environment);
ConfigureDatabase(webApp);

await webApp.RunAsync();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddCustomSwagger();
    services.AddCustomOptions(configuration);
    services.AddCustomServices();
    services.AddCustomDbContext(configuration, typeof(Program).Assembly);
    services.AddCustomJwtBearer(configuration);
    services.AddCustomCors();
    services.AddCustomAntiforgery();
    services.AddHttpClient();
}

void ConfigureLogging(ILoggingBuilder logging, IHostEnvironment env, IConfiguration configuration)
{
    logging.ClearProviders();

    logging.AddDebug();

    if (env.IsDevelopment())
    {
        logging.AddConsole();
    }

    logging.AddConfiguration(configuration.GetSection(key: "Logging"));
}

void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
{
    if (!env.IsDevelopment())
    {
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseMiddleware<ErrorHandlerMiddleware>();
    //app.UseExceptionHandler(appBuilder =>
    //{
    //    appBuilder.Use(async (context, next) =>
    //    {
    //        var error = context.Features[typeof(IExceptionHandlerFeature)] as IExceptionHandlerFeature;

    //        if (error?.Error is SecurityTokenExpiredException)
    //        {
    //            context.Response.StatusCode = 401;
    //            context.Response.ContentType = "application/json";

    //            await context.Response.WriteAsync(JsonSerializer.Serialize(new
    //            {
    //                State = 401,
    //                Msg = "token expired"
    //            }));
    //        }
    //        else if (error?.Error != null)
    //        {
    //            context.Response.StatusCode = 500;
    //            context.Response.ContentType = "application/json";

    //            await context.Response.WriteAsync(JsonSerializer.Serialize(new
    //            {
    //                State = 500,
    //                Msg = error.Error.Message
    //            }));
    //        }
    //        else
    //        {
    //            await next();
    //        }
    //    });
    //});

    app.UseStatusCodePages();

    app.UseCustomSwagger();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();

    app.UseCors(policyName: "CorsPolicy");

    app.UseAuthorization();
}

void ConfigureEndpoints(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
    });

    // catch-all handler for HTML5 client routes - serve index.html
    app.Run(async context =>
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(Path.Combine(env.WebRootPath, path2: "index.html"));
    });
}

void ConfigureDatabase(IApplicationBuilder app)
{
    var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();
    using var scope = scopeFactory.CreateScope();
    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializerService>();
    dbInitializer.Initialize();
    dbInitializer.SeedData();
}