using Abp.Spark.Domain.Menus;
using Abp.Spark.Menus;
using Abp.Spark.Users;
using Abp.Spark.Customers;
using Lion.AbpPro.BasicManagement;
using Lion.AbpPro.DataDictionaryManagement;
using Lion.AbpPro.LanguageManagement;
using Lion.AbpPro.NotificationManagement;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace Abp.Spark.EntityFrameworkCore;

[ConnectionStringName(AbpSparkConsts.ConnectionStringName)]
public class AbpSparkDbContext : AbpDbContext<AbpSparkDbContext>,
    IBasicManagementDbContext,
    INotificationManagementDbContext,
    IDataDictionaryManagementDbContext,
    ILanguageManagementDbContext,
    IPermissionManagementDbContext,
    ISettingManagementDbContext,
    IFeatureManagementDbContext
{
    // Custom entities
    public DbSet<AppUser> AppUsers { get; set; } = null!;
    public DbSet<MenuConfig> MenuConfigs { get; set; } = null!;
    public DbSet<MenuRoleVisibility> MenuRoleVisibilities { get; set; } = null!;

    // BasicManagement (Users, Roles, OrganizationUnits, Audit, Logs, Tenants)
    public DbSet<IdentityUser> Users { get; set; } = null!;
    public DbSet<IdentityRole> Roles { get; set; } = null!;
    public DbSet<IdentityRoleClaim> RoleClaims { get; set; } = null!;
    public DbSet<IdentityUserClaim> UserClaims { get; set; } = null!;
    public DbSet<IdentityUserLogin> UserLogins { get; set; } = null!;
    public DbSet<IdentityUserRole> UserRoles { get; set; } = null!;
    public DbSet<IdentityUserToken> UserTokens { get; set; } = null!;
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; } = null!;
    public DbSet<IdentityLinkUser> LinkUsers { get; set; } = null!;
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; } = null!;
    public DbSet<IdentityClaimType> ClaimTypes { get; set; } = null!;
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; } = null!;
    public DbSet<OrganizationUnitRole> OrganizationUnitRoles { get; set; } = null!;
    public DbSet<IdentitySession> Sessions { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<AuditLogExcelFile> AuditLogExcelFiles { get; set; } = null!;
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; } = null!;
    public DbSet<BackgroundJobRecord> BackgroundJobs { get; set; } = null!;

    // Notifications
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<NotificationSubscription> NotificationSubscriptions { get; set; } = null!;

    // DataDictionary
    public DbSet<DataDictionary> DataDictionaries { get; set; } = null!;

    // Languages
    public DbSet<Language> Languages { get; set; } = null!;
    public DbSet<LanguageText> LanguageTexts { get; set; } = null!;

    // Customers (Manufacturing Demo)
    public DbSet<Customer> Customers { get; set; } = null!;

    // ABP Framework Core tables
    public DbSet<PermissionGroupDefinitionRecord> PermissionGroups { get; set; } = null!;
    public DbSet<PermissionDefinitionRecord> Permissions { get; set; } = null!;
    public DbSet<PermissionGrant> PermissionGrants { get; set; } = null!;
    public DbSet<ResourcePermissionGrant> ResourcePermissionGrants { get; set; } = null!;
    public DbSet<Setting> Settings { get; set; } = null!;
    public DbSet<SettingDefinitionRecord> SettingDefinitionRecords { get; set; } = null!;
    public DbSet<FeatureGroupDefinitionRecord> FeatureGroups { get; set; } = null!;
    public DbSet<FeatureDefinitionRecord> Features { get; set; } = null!;
    public DbSet<FeatureValue> FeatureValues { get; set; } = null!;

    public AbpSparkDbContext(DbContextOptions<AbpSparkDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // 1. 自定义实体
        builder.Entity<AppUser>(b =>
        {
            b.ToTable(AbpSparkConsts.DbTablePrefix + "Users", AbpSparkConsts.DbSchema);
            b.ConfigureByConvention();
            b.Property(x => x.Name).IsRequired().HasMaxLength(64);
            b.Property(x => x.Email).IsRequired().HasMaxLength(128);
            b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(256);
            b.HasIndex(x => x.Email).IsUnique();
        });

        // 2. 菜单模块
        builder.ConfigureMenus();

        // 3. 企业级模块配置
        builder.ConfigureBasicManagement();
        builder.ConfigureNotificationManagement();
        builder.ConfigureDataDictionaryManagement();
        builder.ConfigureLanguageManagement();

        // 4. ABP 核心模块配置
        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureFeatureManagement();
        builder.ConfigureAuditLogging();

        // 5. 制造业客户管理
        builder.ApplyConfiguration(new CustomerConfiguration());
    }
}
