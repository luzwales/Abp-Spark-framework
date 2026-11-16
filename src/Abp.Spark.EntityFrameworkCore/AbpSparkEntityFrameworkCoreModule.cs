using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.MySQL;
using Volo.Abp.EntityFrameworkCore.PostgreSql;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.Modularity;

namespace Abp.Spark.EntityFrameworkCore;

[DependsOn(
    typeof(AbpSparkDomainModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqlServerModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpEntityFrameworkCoreMySQLModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
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
