using Lion.AbpPro.BasicManagement.EntityFrameworkCore;
using Lion.AbpPro.DataDictionaryManagement.EntityFrameworkCore;
using Lion.AbpPro.LanguageManagement.EntityFrameworkCore;
using Lion.AbpPro.NotificationManagement.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.MySQL;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace Abp.Spark.EntityFrameworkCore;

[DependsOn(
    typeof(AbpSparkDomainModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpEntityFrameworkCoreMySQLModule),
    typeof(AbpEntityFrameworkCoreSqliteModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpSettingManagementEntityFrameworkCoreModule),
    typeof(AbpFeatureManagementEntityFrameworkCoreModule),
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(BasicManagementEntityFrameworkCoreModule),
    typeof(NotificationManagementEntityFrameworkCoreModule),
    typeof(DataDictionaryManagementEntityFrameworkCoreModule),
    typeof(LanguageManagementEntityFrameworkCoreModule)
)]
public class AbpSparkEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<AbpSparkDbContext>(options =>
        {
            options.AddDefaultRepositories(includeAllEntities: true);
        });

        Configure<AbpDbContextOptions>(options =>
        {
            var configuration = context.Services.GetConfiguration();
            var dbProvider = (configuration["Database:Provider"] ?? "SqlServer").Trim().ToLowerInvariant();

            switch (dbProvider)
            {
                case "postgresql":
                case "postgres":
                case "npgsql":
                    options.UseNpgsql();
                    break;

                case "mysql":
                    options.UseMySQL();
                    break;

                case "sqlite":
                    options.UseSqlite();
                    break;

                case "sqlserver":
                default:
                    options.UseSqlServer();
                    break;
            }
        });
    }
}
