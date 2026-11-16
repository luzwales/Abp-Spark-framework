using System.Security.Cryptography;
using System.Text;
using Abp.Spark.Domain.Menus;
using Abp.Spark.Menus;
using Abp.Spark.Users;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Abp.Spark;

public class AbpSparkDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<AppUser, Guid> _userRepository;
    private readonly IRepository<MenuConfig, Guid> _menuRepository;
    private readonly IGuidGenerator _guidGenerator;

    public AbpSparkDataSeedContributor(
        IRepository<AppUser, Guid> userRepository,
        IRepository<MenuConfig, Guid> menuRepository,
        IGuidGenerator guidGenerator)
    {
        _userRepository = userRepository;
        _menuRepository = menuRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedUsersAsync();
        await SeedMenusAsync();
    }

    private async Task SeedUsersAsync()
    {
        if (await _userRepository.GetCountAsync() > 0)
        {
            return;
        }

        var adminUser = new AppUser(
            _guidGenerator.Create(),
            "Andrew Lu",
            "andrew@example.com",
            HashPassword("Password123!"),
            role: "Admin"
        );

        await _userRepository.InsertAsync(adminUser, autoSave: true);
    }

    private async Task SeedMenusAsync()
    {
        if (await _menuRepository.GetCountAsync() > 0)
        {
            return;
        }

        // 1. 仪表盘
        var dashboardMenu = new MenuConfig(
            _guidGenerator.Create(),
            parentId: null,
            name: "Dashboard",
            label: "Dashboard",
            icon: "LayoutDashboard",
            route: "/dashboard",
            type: SidebarItemType.Link,
            sortOrder: 10
        );
        await _menuRepository.InsertAsync(dashboardMenu, autoSave: false);

        // 2. 系统管理组
        var systemMenuId = _guidGenerator.Create();
        var systemMenu = new MenuConfig(
            systemMenuId,
            parentId: null,
            name: "SystemManagement",
            label: "System",
            icon: "Settings",
            route: null,
            type: SidebarItemType.Group,
            sortOrder: 20
        );
        await _menuRepository.InsertAsync(systemMenu, autoSave: false);

        // 系统管理子菜单 (符合 Lucide 标准图标)
        var subMenus = new (string name, string label, string icon, string route, int sortOrder)[]
        {
            ("UserManagement", "Users", "Users", "/users", 1),
            ("RoleManagement", "Roles", "ShieldCheck", "/roles", 2),
            ("OrganizationManagement", "OrganizationUnits", "Network", "/organization-units", 3),
            ("AuditLogManagement", "AuditLogs", "FileText", "/audit-logs", 4),
            ("NotificationManagement", "Notifications", "Bell", "/notifications", 5),
            ("DataDictionaryManagement", "DataDictionaries", "BookOpen", "/data-dictionaries", 6),
            ("TenantManagement", "Tenants", "Building", "/tenants", 7)
        };

        foreach (var item in subMenus)
        {
            var child = new MenuConfig(
                _guidGenerator.Create(),
                parentId: systemMenuId,
                name: item.name,
                label: item.label,
                icon: item.icon,
                route: item.route,
                type: SidebarItemType.Link,
                sortOrder: item.sortOrder
            );
            await _menuRepository.InsertAsync(child, autoSave: false);
        }
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
