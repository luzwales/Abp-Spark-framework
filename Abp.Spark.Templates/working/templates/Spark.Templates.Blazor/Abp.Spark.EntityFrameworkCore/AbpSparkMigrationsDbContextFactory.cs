using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Abp.Spark.EntityFrameworkCore;

public class AbpSparkMigrationsDbContextFactory : IDesignTimeDbContextFactory<AbpSparkDbContext>
{
    public AbpSparkDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var provider = (configuration["Database:Provider"] ?? "SqlServer").Trim().ToLowerInvariant();
        var connectionString = configuration.GetConnectionString(AbpSparkConsts.ConnectionStringName);

        var builder = new DbContextOptionsBuilder<AbpSparkDbContext>();

        switch (provider)
        {
            case "postgresql":
            case "postgres":
            case "npgsql":
                builder.UseNpgsql(connectionString);
                break;
            case "mysql":
                builder.UseMySQL(connectionString);
                break;
            case "sqlite":
                builder.UseSqlite(connectionString);
                break;
            case "sqlserver":
            default:
                builder.UseSqlServer(connectionString);
                break;
        }

        return new AbpSparkDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../Abp.Spark.Web/");
        if (!Directory.Exists(basePath))
        {
            basePath = Directory.GetCurrentDirectory();
        }

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true);

        return builder.Build();
    }
}
