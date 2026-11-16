using Abp.Spark.EntityFrameworkCore;
using Abp.Spark.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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

        // 自动初始化数据库架构并种子填充数据
        using (var scope = context.ServiceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AbpSparkDbContext>();
            dbContext.Database.EnsureCreated();
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
            endpoints.MapPost("/logout", async (HttpContext ctx) =>
            {
                await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.Redirect("/");
            });
            endpoints.MapRazorComponents<Abp.Spark.Pages.PageRoutes>();
        });
    }
}
