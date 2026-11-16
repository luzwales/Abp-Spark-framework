# 技术设计方案：ABP vNext Pro DDD 分层架构与现代化 Blazor 单体融合

- **日期**：2026-11-16
- **架构模式**：单体进程内托管（In-Process Monolith）+ 标准 DDD 领域驱动分层
- **适用框架**：.NET 10.0 / ABP vNext 10.x / Blazor SSR / DaisyUI + Tailwind + Lucide

---

## 1. 架构目标与设计原则

1. **企业级标准 DDD 分层**：
   参考 `abp-vnext-pro` 的分层范式，分离领域层（Domain）、合约层（Application.Contracts）、应用服务层（Application）、仓储持久化层（EntityFrameworkCore）、对外接口层（HttpApi）与展示宿主层（Web/Blazor）。
2. **多数据库提供程序无缝切换（Multi-DB Provider）**：
   在 `appsettings.json` 中增加 `Database:Provider` 配置项，支持一键热切：
   * `SqlServer`（默认，面向制造企业生产级稳定环境）
   * `PostgreSql`（开源高性能场景）
   * `MySql`（传统云端多实例部署场景）
   * `Sqlite`（本地轻量快速开发、离线边缘节点）
3. **单体进程内托管（In-Process Monolith）**：
   Blazor 页面与 ABP Host 统一运行在 `Abp.Spark.Web` 宿主中。
   * **内部调用**：Razor 组件直接通过 DI 注入 `IAppService`（如 `@inject IUserAppService`），零 HTTP 开销、毫秒级响应、具备完整事务工作单元（UOW）；
   * **外部开放**：同时配置 Swagger UI 与控制器端点，自动对外暴露 REST API。
4. **前端资产完整继承**：
   完整保留已实现的 Vristo 现代化视觉规范、Lucide 官方图标系统、DaisyUI 双主题、Morphicons 主题物理变形动效以及 JSON 驱动的多语言 i18n 体系。

---

## 2. 解决方案与项目分层拓扑

```
Abp-Spark-framework / templates /
└── MyCompanyName.MyProjectName.slnx (或 .sln)
    ├── src/
    │   ├── 1. Domain.Shared/          # 常量、通用枚举、全局错误码、数据字典定义
    │   ├── 2. Domain/                 # 领域实体、聚合根、领域服务接口与实现、仓储接口
    │   ├── 3. Application.Contracts/  # 应用服务契约 (IAppService) 与 DTO
    │   ├── 4. Application/            # 应用服务实现、Mapperly 对象映射、业务规则验证
    │   ├── 5. EntityFrameworkCore/    # DbContext、FluentAPI 映射、4大数据库 Provider 适配
    │   ├── 6. HttpApi/                # ASP.NET Core API 控制器 (REST API 对外暴露)
    │   └── 7. Web/                    # 统一启动宿主：集成 Blazor 前端、Swagger、静态资源与 Cookie 认证
    └── test/                          # 基础测试套件
```

### 项目依赖关系图：
```
       ┌────────────────────────┐
       │   Abp.Spark.Web (Host) │
       └───────┬───┬───┬────────┘
               │   │   │
        ┌──────┘   │   └────────────────┐
        ▼          ▼                    ▼
   [HttpApi]  [Application]   [EntityFrameworkCore]
        │          │                    │
        ▼          ▼                    ▼
[Application.Contracts] ────────►   [Domain]
        │                               │
        └──────────────┬────────────────┘
                       ▼
               [Domain.Shared]
```

---

## 3. 多数据库切换机制设计（Multi-Database Provider）

### 3.1 配置文件定义（`appsettings.json`）
```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=AbpSparkDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Database": {
    "Provider": "SqlServer" 
    // 支持可选值: "SqlServer" | "PostgreSql" | "MySql" | "Sqlite"
  }
}
```

### 3.2 动态提供程序分流实现（`EntityFrameworkCoreModule.cs`）
在 `Abp.Spark.EntityFrameworkCore` 层统一管理数据库提供程序扩展包：
* `Volo.Abp.EntityFrameworkCore.SqlServer`
* `Volo.Abp.EntityFrameworkCore.PostgreSql`
* `Volo.Abp.EntityFrameworkCore.MySQL`
* `Volo.Abp.EntityFrameworkCore.Sqlite`

核心分流逻辑：
```csharp
Configure<AbpDbContextOptions>(options =>
{
    var configuration = context.Services.GetConfiguration();
    var dbProvider = (configuration["Database:Provider"] ?? "SqlServer").ToLowerInvariant();

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
```

---

## 4. 单体宿主（Web）与 Blazor 前端生命周期整合

### 4.1 根模块设计（`AbpSparkWebModule.cs`）
```csharp
[DependsOn(
    typeof(AbpSparkHttpApiModule),
    typeof(AbpSparkApplicationModule),
    typeof(AbpSparkEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpSwashbuckleModule)
)]
public class AbpSparkWebModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        var configuration = context.Services.GetConfiguration();

        // 1. 注册 Swagger API 文档
        services.AddAbpSwaggerGenWithOAuth(...);

        // 2. 注册 Blazor 服务端渲染与 Razor 组件
        services.AddRazorComponents()
                .AddInteractiveServerComponents();

        // 3. 注册已有的 JSON 本地化服务与 Vite 前端集成
        services.AddHttpContextAccessor();
        services.AddScoped<I18nService>();
        services.AddViteServices();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        // Swagger 端点
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Abp.Spark API");
        });

        // 路由映射：同时支持 REST API 与 Blazor 前端页面
        app.UseConfiguredEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapRazorComponents<App>()
                     .AddInteractiveServerRenderMode();
        });
    }
}
```

### 4.2 前端 Razor 组件直接调用 AppService
在 Razor 页面中，直接通过依赖注入获取强类型业务服务：
```razor
@page "/dashboard"
@inject IDashboardAppService DashboardAppService
@inject I18nService L

<PageTitle>@L["dashboard"]</PageTitle>

<div class="grid grid-cols-1 md:grid-cols-3 gap-6">
    <!-- 极速进程内调用，无 HTTP 序列化/反序列化开销 -->
    @foreach(var kpi in kpiStats)
    {
        <div class="card bg-white dark:bg-[#0e1726] shadow-sm rounded-2xl p-6">
            <h4 class="text-sm font-semibold text-[#888ea8]">@kpi.Title</h4>
            <div class="text-2xl font-bold mt-2">@kpi.Value</div>
        </div>
    }
</div>

@code {
    private List<KpiStatDto> kpiStats = new();

    protected override async Task OnInitializedAsync()
    {
        kpiStats = await DashboardAppService.GetSummaryStatsAsync();
    }
}
```

---

## 5. 实施里程碑计划（Milestones）

| 阶段 | 核心任务 | 交付成果 |
| :--- | :--- | :--- |
| **M1: DDD 基础类库骨架搭建** | 建立 `Domain.Shared`、`Domain`、`Contracts`、`Application`、`EFCore`、`HttpApi` 基础工程与 GlobalUsings。 | 6 个分层类库通过 `dotnet build` 编译无误。 |
| **M2: EFCore 与多数据库 Provider 注入** | 编写 `AbpDbContext`，接入软删除与审计日志，在 `appsettings.json` 落地多数据库动态 Provider 分流机制。 | 可通过修改配置在 SqlServer / Sqlite 间无缝切换连接。 |
| **M3: Web 单体宿主与 Blazor 接入** | 移植 `Assets/`、`Locales/`、`Pages/` 至 `Web` 宿主，接入 `AbpSparkWebModule` 与 Autofac 容器。 | `dotnet run` 启动，浏览器正常呈现前端页面，并可访问 `/swagger`。 |
| **M4: 业务数据流打通与验证** | 建立用户与系统概览应用服务（`IUserAppService`, `IDashboardAppService`），在 Razor 页面中通过 DI 真实渲染数据。 | 前端各功能模块与 ABP 后端业务服务完全打通。 |
| **M5: 模板回填与 CLI 集成** | 将整套工程转换为 `Abp.Spark.Templates`，支持 `spark new <name>` 一键脚手架生成。 | CLI 生成的全新 ABP+Blazor 项目开箱即用。 |

---

## 6. 风险评估与应对策略

1. **多数据库提供程序兼容性（EF Core Migration）**：
   * *风险*：不同数据库的方言与类型映射存在细微差异。
   * *应对*：实体配置采用标准 Fluent API 基础类型；开发期首推 SqlServer / Sqlite 作为基准测试环境，提供干净的数据迁移上下文。
2. **Blazor 渲染生命周期与 ABP 工作单元（UOW）**：
   * *风险*：Blazor 异步渲染可能脱离请求作用域的 DbContext 范围。
   * *应对*：AppService 严格遵循 ABP 的 `[UnitOfWork]` 契约，在方法执行期完成数据拉取，返回已加载的 DTO，避免在 Razor 模板中延迟加载导航属性。
