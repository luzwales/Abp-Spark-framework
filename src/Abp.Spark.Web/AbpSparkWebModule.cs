using Abp.Spark.EntityFrameworkCore;
using Abp.Spark.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Vite.AspNetCore;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;

namespace Abp.Spark;

[DependsOn(
    typeof(AbpSparkHttpApiModule),
    typeof(AbpSparkApplicationModule),
    typeof(AbpSparkEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule)
)]
public class AbpSparkWebModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        var configuration = context.Services.GetConfiguration();

        // 1. 注册 Swagger 生成器
        services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() { Title = "Abp.Spark API", Version = "v1" });
            options.DocInclusionPredicate((docName, description) => true);
            options.CustomSchemaIds(type => type.FullName);
        });

        // 2. 注册 Cookie 认证
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                });

        // 3. 注册 Blazor 服务端渲染与 Razor 组件
        services.AddRazorComponents();
        services.AddAntiforgery();

        // 4. 注册 Vite 前端构建中间件与轻量 JSON i18n
        services.AddViteServices();
        services.AddHttpContextAccessor();
        services.AddScoped<I18nService>();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        // 自动增量迁移数据库架构并种子填充数据
        using (var scope = context.ServiceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AbpSparkDbContext>();
            var config = context.ServiceProvider.GetRequiredService<IConfiguration>();
            var provider = (config["Database:Provider"] ?? "SqlServer").Trim().ToLowerInvariant();

            if (provider == "sqlite")
            {
                dbContext.Database.EnsureCreated();
            }
            else
            {
                // SqlServer / PostgreSQL / MySQL 执行标准增量迁移
                Volo.Abp.Threading.AsyncHelper.RunSync(() => dbContext.Database.MigrateAsync());
            }

            var dataSeeder = scope.ServiceProvider.GetRequiredService<Volo.Abp.Data.IDataSeeder>();
            Volo.Abp.Threading.AsyncHelper.RunSync(() => dataSeeder.SeedAsync(new Volo.Abp.Data.DataSeedContext()));
        }

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();
        app.UseCorrelationId();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();

        // Swagger 端点
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Abp.Spark API");
        });

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseUnitOfWork();

        app.UseConfiguredEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapPost("/login-direct", async (HttpContext ctx, Abp.Spark.Users.IUserAppService userAppService) =>
            {
                var form = await ctx.Request.ReadFormAsync();
                var account = (form["Email"].FirstOrDefault() ?? form["username"].FirstOrDefault() ?? "").Trim();
                var password = form["Password"].FirstOrDefault() ?? form["password"].FirstOrDefault() ?? "";
                var rememberMe = (form["RememberMe"].FirstOrDefault() ?? "false").Equals("true", StringComparison.OrdinalIgnoreCase);

                try
                {
                    var user = await userAppService.LoginAsync(new Abp.Spark.Users.LoginInputDto { Email = account, Password = password });
                    var claims = new System.Collections.Generic.List<System.Security.Claims.Claim>
                    {
                        new(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new(System.Security.Claims.ClaimTypes.Name, user.Name),
                        new(System.Security.Claims.ClaimTypes.Email, user.Email),
                        new(System.Security.Claims.ClaimTypes.Role, user.Role),
                        new(System.Security.Claims.ClaimTypes.Role, "Admin"),
                        new(System.Security.Claims.ClaimTypes.Role, "admin")
                    };
                    var identity = new System.Security.Claims.ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await ctx.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new System.Security.Claims.ClaimsPrincipal(identity),
                        new AuthenticationProperties
                        {
                            IsPersistent = rememberMe,
                            IssuedUtc = DateTimeOffset.UtcNow,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                        });
                    return Results.Redirect("/dashboard");
                }
                catch (Exception ex)
                {
                    return Results.Redirect($"/login?error={Uri.EscapeDataString(ex.Message)}");
                }
            }).DisableAntiforgery();
            endpoints.MapPost("/logout", async (HttpContext ctx) =>
            {
                await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.Redirect("/");
            });
            endpoints.MapRazorComponents<Abp.Spark.Pages.PageRoutes>();
        });
    }
}
