using System.Security.Cryptography;
using System.Text;
using Abp.Spark.Domain.Menus;
using Abp.Spark.Menus;
using Abp.Spark.Users;
using Lion.AbpPro.DataDictionaryManagement.DataDictionaries;
using Lion.AbpPro.DataDictionaryManagement.DataDictionaries.Aggregates;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace Abp.Spark;

public class AbpSparkDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<AppUser, Guid> _userRepository;
    private readonly IRepository<MenuConfig, Guid> _menuRepository;
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;
    private readonly IDataDictionaryRepository _dataDictionaryRepository;
    private readonly IGuidGenerator _guidGenerator;

    public AbpSparkDataSeedContributor(
        IRepository<AppUser, Guid> userRepository,
        IRepository<MenuConfig, Guid> menuRepository,
        OrganizationUnitManager organizationUnitManager,
        IOrganizationUnitRepository organizationUnitRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        IGuidGenerator guidGenerator)
    {
        _userRepository = userRepository;
        _menuRepository = menuRepository;
        _organizationUnitManager = organizationUnitManager;
        _organizationUnitRepository = organizationUnitRepository;
        _dataDictionaryRepository = dataDictionaryRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedUsersAsync();
        await SeedMenusAsync();
        await SeedOrganizationUnitsAsync();
        await SeedDataDictionariesAsync();
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

    private async Task SeedOrganizationUnitsAsync()
    {
        if (await _organizationUnitRepository.GetCountAsync() > 0)
        {
            return;
        }

        // 1. 根部门：智能制造运营中心
        var rootOu = new OrganizationUnit(
            _guidGenerator.Create(),
            "智能制造运营中心 (Operations Center)",
            parentId: null
        );
        rootOu.ExtraProperties["ManagerName"] = "Andrew Lu";
        rootOu.ExtraProperties["ManagerLevel"] = 1;
        await _organizationUnitManager.CreateAsync(rootOu);

        // 2. 根部门：品质保证与合规中心
        var qaOu = new OrganizationUnit(
            _guidGenerator.Create(),
            "品质保证与合规中心 (QA & Compliance)",
            parentId: null
        );
        qaOu.ExtraProperties["ManagerName"] = "Operator Li";
        qaOu.ExtraProperties["ManagerLevel"] = 1;
        await _organizationUnitManager.CreateAsync(qaOu);
    }

    private async Task SeedDataDictionariesAsync()
    {
        // 1. 工单状态字典 (WorkOrderStatus)
        var orderStatus = await _dataDictionaryRepository.FindByCodeAsync("WorkOrderStatus");
        if (orderStatus == null)
        {
            var dict = new DataDictionary(_guidGenerator.Create(), "WorkOrderStatus", "工单生产状态", "制造业工单流转全生命周期状态");
            dict.AddDetail(_guidGenerator.Create(), "PENDING", "待下发", 1, "工单已创建等待排产调度");
            dict.AddDetail(_guidGenerator.Create(), "IN_PROGRESS", "生产执行中", 2, "产线工位正在报工执行中");
            dict.AddDetail(_guidGenerator.Create(), "QA_INSPECTING", "品检抽检中", 3, "质检员正在执行工序抽样检验");
            dict.AddDetail(_guidGenerator.Create(), "COMPLETED", "已完工入库", 4, "已报工完工并入成品库");
            dict.AddDetail(_guidGenerator.Create(), "CANCELLED", "已作废终止", 5, "因物料异常或计划变更作废", false);
            await _dataDictionaryRepository.InsertAsync(dict);
        }

        // 2. 缺陷严重度等级 (DefectLevel)
        var defectLevel = await _dataDictionaryRepository.FindByCodeAsync("DefectLevel");
        if (defectLevel == null)
        {
            var dict = new DataDictionary(_guidGenerator.Create(), "DefectLevel", "质量缺陷等级", "产品QC巡检与CAPA质量事故等级分类");
            dict.AddDetail(_guidGenerator.Create(), "CRITICAL", "致命缺陷 (Critical)", 1, "影响人身安全、法规合规或核心电气性能");
            dict.AddDetail(_guidGenerator.Create(), "MAJOR", "严重缺陷 (Major)", 2, "导致功能降级或明显外观异常");
            dict.AddDetail(_guidGenerator.Create(), "MINOR", "轻微缺陷 (Minor)", 3, "工艺毛刺、不影响功能的微小外观瑕疵");
            await _dataDictionaryRepository.InsertAsync(dict);
        }

        // 3. 质检工序类型 (InspectionType)
        var inspectionType = await _dataDictionaryRepository.FindByCodeAsync("InspectionType");
        if (inspectionType == null)
        {
            var dict = new DataDictionary(_guidGenerator.Create(), "InspectionType", "检验工序类型", "质量控制各关键检验节点");
            dict.AddDetail(_guidGenerator.Create(), "IQC", "来料进料检验 (IQC)", 1, "原材料入库验收抽样检查");
            dict.AddDetail(_guidGenerator.Create(), "IPQC", "制程巡回检验 (IPQC)", 2, "生产车间巡检与首件检验");
            dict.AddDetail(_guidGenerator.Create(), "FQC", "最终成品检验 (FQC)", 3, "总装下线最终检验");
            dict.AddDetail(_guidGenerator.Create(), "OQC", "出货放行检验 (OQC)", 4, "打包出货前封箱复检");
            await _dataDictionaryRepository.InsertAsync(dict);
        }
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
