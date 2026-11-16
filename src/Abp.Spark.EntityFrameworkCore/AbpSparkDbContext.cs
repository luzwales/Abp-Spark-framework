using Abp.Spark.Users;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Abp.Spark.EntityFrameworkCore;

[ConnectionStringName(AbpSparkConsts.ConnectionStringName)]
public class AbpSparkDbContext : AbpDbContext<AbpSparkDbContext>
{
    public DbSet<AppUser> Users { get; set; } = null!;

    public AbpSparkDbContext(DbContextOptions<AbpSparkDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>(b =>
        {
            b.ToTable(AbpSparkConsts.DbTablePrefix + "Users", AbpSparkConsts.DbSchema);
            b.ConfigureByConvention(); // 自动配置审计、软删除等字段
            b.Property(x => x.Name).IsRequired().HasMaxLength(64);
            b.Property(x => x.Email).IsRequired().HasMaxLength(128);
            b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(256);
            b.HasIndex(x => x.Email).IsUnique();
        });
    }
}
