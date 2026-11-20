using Abp.Spark.Domain.Menus;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Abp.Spark.EntityFrameworkCore;

public static class MenuConfigDbContextModelCreatingExtensions
{
    public static void ConfigureMenus(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<MenuConfig>(b =>
        {
            b.ToTable(AbpSparkConsts.DbTablePrefix + "MenuConfigs", AbpSparkConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(128);
            b.Property(x => x.Label).IsRequired().HasMaxLength(128);
            b.Property(x => x.Icon).HasMaxLength(64);
            b.Property(x => x.Route).HasMaxLength(256);
            b.Property(x => x.PermissionName).HasMaxLength(128);

            b.HasMany(x => x.Children)
             .WithOne(x => x.Parent)
             .HasForeignKey(x => x.ParentId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.RoleVisibilities)
             .WithOne(x => x.MenuConfig)
             .HasForeignKey(x => x.MenuConfigId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MenuRoleVisibility>(b =>
        {
            b.ToTable(AbpSparkConsts.DbTablePrefix + "MenuRoleVisibilities", AbpSparkConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.RoleName).IsRequired().HasMaxLength(128);
            b.HasIndex(x => new { x.MenuConfigId, x.RoleName });
        });
    }
}
