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

        // 1. 注册 Swagger 生成器 (支持 Bearer JWT 调试授权)
        services.AddAbpSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() { Title = "Abp.Spark API", Version = "v1" });
            options.DocInclusionPredicate((docName, description) => true);
            options.CustomSchemaIds(type => type.FullName);

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "请输入 JWT Bearer 授权令牌: Bearer {token}",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
        });

        // 2. 注册 Cookie 与 JWT Bearer 混合认证
        var jwtSecurityKey = configuration["Jwt:SecurityKey"] ?? "AbpSparkFrameworkSecretKeyForJwtAuthentication2026!@";
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "Abp.Spark";
        var jwtAudience = configuration["Jwt:Audience"] ?? "Abp.Spark.Client";

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
        })
        .AddJwtBearer(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecurityKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };
        });

        // 3. 注册 Blazor 服务端渲染与 Interactive Server Razor 组件
        services.AddRazorComponents()
            .AddInteractiveServerComponents();
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
            // 语言动态切换服务端端点 (支持无 JS 刷新重定向)
            endpoints.MapGet("/change-language", (HttpContext ctx, string culture, string? returnUrl) =>
            {
                var target = string.IsNullOrWhiteSpace(culture) ? "en" : culture.Trim().ToLowerInvariant();
                var normalized = target.StartsWith("en") ? "en" : "zh";

                var cookieOptions = new CookieOptions
                {
                    Path = "/",
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                };

                // 1. 设置系统轻量 JSON i18n Cookie
                ctx.Response.Cookies.Append("spark_locale", normalized, cookieOptions);

                // 2. 设置 ABP / ASP.NET Core 标准本地化 Cookie
                var abpCulture = normalized == "en" ? "en" : "zh-Hans";
                ctx.Response.Cookies.Append(
                    Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName,
                    Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(new Microsoft.AspNetCore.Localization.RequestCulture(abpCulture)),
                    cookieOptions);

                var redirect = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
                return Results.LocalRedirect(redirect);
            });

            // 标准 JWT Token 签发 Minimal API (支持 REST / Swagger / 跨端客户端)
            endpoints.MapPost("/api/account/token", async ([Microsoft.AspNetCore.Mvc.FromBody] Abp.Spark.Users.LoginInputDto input, Abp.Spark.Users.IUserAppService userAppService) =>
            {
                var result = await userAppService.LoginJwtAsync(input);
                return Results.Ok(result);
            }).DisableAntiforgery();

            endpoints.MapPost("/logout", async (HttpContext ctx) =>
            {
                await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.Redirect("/");
            });
            endpoints.MapRazorComponents<Abp.Spark.Pages.PageRoutes>()
                .AddInteractiveServerRenderMode();
        });
    }
}
