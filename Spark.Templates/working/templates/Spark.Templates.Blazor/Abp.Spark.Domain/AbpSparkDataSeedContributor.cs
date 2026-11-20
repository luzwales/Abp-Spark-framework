using System.Security.Cryptography;
using System.Text;
using Abp.Spark.Domain.Menus;
using Abp.Spark.Menus;
using Abp.Spark.Users;
using Abp.Spark.Customers;
using Lion.AbpPro.DataDictionaryManagement.DataDictionaries;
using Lion.AbpPro.DataDictionaryManagement.DataDictionaries.Aggregates;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;

namespace Abp.Spark;

public class AbpSparkDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<AppUser, Guid> _userRepository;
    private readonly IRepository<MenuConfig, Guid> _menuRepository;
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;
    private readonly IDataDictionaryRepository _dataDictionaryRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<PermissionGrant, Guid> _permissionGrantRepository;
    private readonly IGuidGenerator _guidGenerator;

    public AbpSparkDataSeedContributor(
        IRepository<AppUser, Guid> userRepository,
        IRepository<MenuConfig, Guid> menuRepository,
        OrganizationUnitManager organizationUnitManager,
        IOrganizationUnitRepository organizationUnitRepository,
        IDataDictionaryRepository dataDictionaryRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<PermissionGrant, Guid> permissionGrantRepository,
        IGuidGenerator guidGenerator)
    {
        _userRepository = userRepository;
        _menuRepository = menuRepository;
        _organizationUnitManager = organizationUnitManager;
        _organizationUnitRepository = organizationUnitRepository;
        _dataDictionaryRepository = dataDictionaryRepository;
        _customerRepository = customerRepository;
        _permissionGrantRepository = permissionGrantRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await SeedUsersAsync();
        await SeedMenusAsync();
        await SeedOrganizationUnitsAsync();
        await SeedDataDictionariesAsync();
        await SeedCustomersAsync();
        await SeedPermissionsAsync();
    }

    private async Task SeedUsersAsync()
    {
        // 1. 标准 ABP 默认超管种子: admin / 1q2w3E* (Role: Admin)
        var defaultAdmin = await _userRepository.FirstOrDefaultAsync(u => u.Name == "admin" || u.Email == "admin@abp.io");
        if (defaultAdmin == null)
        {
            defaultAdmin = new AppUser(
                _guidGenerator.Create(),
                name: "admin",
                email: "admin@abp.io",
                passwordHash: HashPassword("1q2w3E*"),
                role: "Admin"
            );
            await _userRepository.InsertAsync(defaultAdmin, autoSave: true);
        }
        else
        {
            defaultAdmin.PasswordHash = HashPassword("1q2w3E*");
            defaultAdmin.Role = "Admin";
            defaultAdmin.IsActive = true;
            await _userRepository.UpdateAsync(defaultAdmin, autoSave: true);
        }

        // 2. 兼容保留 Andrew Lu 用户
        var andrewUser = await _userRepository.FirstOrDefaultAsync(u => u.Email == "andrew@example.com");
        if (andrewUser == null)
        {
            andrewUser = new AppUser(
                _guidGenerator.Create(),
                name: "Andrew Lu",
                email: "andrew@example.com",
                passwordHash: HashPassword("Password123!"),
                role: "Admin"
            );
            await _userRepository.InsertAsync(andrewUser, autoSave: true);
        }
    }

    private async Task SeedMenusAsync()
    {
        var existingMenus = await _menuRepository.GetListAsync();
        if (existingMenus.Any())
        {
            if (!existingMenus.Any(m => m.Name == "MenuManagement" || m.Route == "/menus"))
            {
                var systemGroup = existingMenus.FirstOrDefault(m => m.Name == "SystemManagement" && m.Type == SidebarItemType.Group);
                if (systemGroup != null)
                {
                    var menuManagement = new MenuConfig(
                        _guidGenerator.Create(),
                        parentId: systemGroup.Id,
                        name: "MenuManagement",
                        label: "Menus",
                        icon: "Menu",
                        route: "/menus",
                        type: SidebarItemType.Link,
                        sortOrder: 9,
                        permissionName: "SparkSystem.Menus"
                    );
                    await _menuRepository.InsertAsync(menuManagement, autoSave: true);
                }
            }
            else
            {
                var existing = existingMenus.FirstOrDefault(m => m.Name == "MenuManagement" || m.Route == "/menus");
                if (existing != null && string.IsNullOrEmpty(existing.PermissionName))
                {
                    existing.PermissionName = "SparkSystem.Menus";
                    await _menuRepository.UpdateAsync(existing, autoSave: true);
                }
            }
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
            ("TenantManagement", "Tenants", "Building", "/tenants", 7),
            ("LanguageManagement", "Languages", "Languages", "/languages", 8),
            ("MenuManagement", "Menus", "Menu", "/menus", 9)
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
                sortOrder: item.sortOrder,
                permissionName: item.name == "MenuManagement" ? "SparkSystem.Menus" : null
            );
            await _menuRepository.InsertAsync(child, autoSave: false);
        }

        // 3. 智能制造业务组
        var mfgMenuId = _guidGenerator.Create();
        var mfgMenu = new MenuConfig(
            mfgMenuId,
            parentId: null,
            name: "Manufacturing",
            label: "Manufacturing",
            icon: "Factory",
            route: null,
            type: SidebarItemType.Group,
            sortOrder: 15
        );
        await _menuRepository.InsertAsync(mfgMenu, autoSave: false);

        var custMenu = new MenuConfig(
            _guidGenerator.Create(),
            parentId: mfgMenuId,
            name: "CustomerManagement",
            label: "Customers",
            icon: "Contact",
            route: "/customers",
            type: SidebarItemType.Link,
            sortOrder: 1
        );
        await _menuRepository.InsertAsync(custMenu, autoSave: false);
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

    private async Task SeedCustomersAsync()
    {
        if (await _customerRepository.GetCountAsync() > 0)
        {
            return;
        }

        var sampleCustomers = new List<Customer>
        {
            new Customer(
                _guidGenerator.Create(),
                code: "CUST-CATL-001",
                name: "宁德时代新能源科技股份有限公司",
                shortName: "宁德时代",
                taxNumber: "91350900587527783P",
                industryType: "新能源汽车",
                contactPerson: "林工 (供应链采购总监)",
                contactPhone: "13959328888",
                contactEmail: "supply_chain@catl.com",
                creditLevel: "AAA",
                settlementMethod: "月结60天",
                isActive: true,
                remark: "战略级客户，动力电池 PACK 壳体加工核心直供商"
            ),
            new Customer(
                _guidGenerator.Create(),
                code: "CUST-BYD-002",
                name: "比亚迪汽车工业有限公司",
                shortName: "比亚迪",
                taxNumber: "9144030079174192XF",
                industryType: "新能源汽车",
                contactPerson: "王经理 (整车SQE部)",
                contactPhone: "13807559988",
                contactEmail: "sqe_byd@byd.com",
                creditLevel: "AAA",
                settlementMethod: "月结60天",
                isActive: true,
                remark: "新能源乘用车三电系统五金压铸冲压件重点采购"
            ),
            new Customer(
                _guidGenerator.Create(),
                code: "CUST-HW-003",
                name: "华为数字能源技术有限公司",
                shortName: "华为数字能源",
                taxNumber: "91440300MA5GMU8M7N",
                industryType: "光伏储能",
                contactPerson: "赵专家 (结构件质量把关人)",
                contactPhone: "13702886677",
                contactEmail: "quality_hw@huawei.com",
                creditLevel: "AAA",
                settlementMethod: "月结30天",
                isActive: true,
                remark: "高压逆变器与工商业储能柜机箱高精密钣金采购"
            ),
            new Customer(
                _guidGenerator.Create(),
                code: "CUST-GOER-004",
                name: "歌尔股份有限公司",
                shortName: "歌尔股份",
                taxNumber: "913707007292552802",
                industryType: "消费电子",
                contactPerson: "孙主管 (精密模具采购)",
                contactPhone: "13605361122",
                contactEmail: "goertek_mould@goertek.com",
                creditLevel: "AA",
                settlementMethod: "月结30天",
                isActive: true,
                remark: "VR/AR 声学精密结构件与微型注塑加工"
            ),
            new Customer(
                _guidGenerator.Create(),
                code: "CUST-NAURA-005",
                name: "北方华创科技集团股份有限公司",
                shortName: "北方华创",
                taxNumber: "91110000722614912Y",
                industryType: "半导体装备",
                contactPerson: "陈博士 (半导体核心器件供应链)",
                contactPhone: "13501003344",
                contactEmail: "semicon@naura.com",
                creditLevel: "AA",
                settlementMethod: "款到发货",
                isActive: true,
                remark: "晶圆刻蚀机与薄膜沉积设备高洁净超精密机械腔体件"
            )
        };

        foreach (var c in sampleCustomers)
        {
            await _customerRepository.InsertAsync(c, autoSave: false);
        }
    }

    private async Task SeedPermissionsAsync()
    {
        var allPermissions = new[]
        {
            "SparkSystem.Menus",
            "SparkSystem.Menus.Create",
            "SparkSystem.Menus.Update",
            "SparkSystem.Menus.Delete",
            "SparkManufacturing.Customers",
            "SparkManufacturing.Customers.Create",
            "SparkManufacturing.Customers.Update",
            "SparkManufacturing.Customers.Delete",
            "SparkManufacturing.Customers.ManageStatus"
        };

        var roles = new[] { "Admin", "admin" };
        foreach (var role in roles)
        {
            foreach (var p in allPermissions)
            {
                if (!await _permissionGrantRepository.AnyAsync(x => x.Name == p && x.ProviderName == "R" && x.ProviderKey == role))
                {
                    await _permissionGrantRepository.InsertAsync(
                        new PermissionGrant(_guidGenerator.Create(), p, "R", role),
                        autoSave: false
                    );
                }
            }
        }
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
