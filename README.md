# Abp-Spark-framework

**Abp-Spark-framework** 是基于 **.NET 10**、**ABP vNext 10.7.1** 与 **Lion.AbpPro 10.7.0.1** 的企业级单体与全栈开发脚手架，融合了 DDD（领域驱动设计）7层架构、Blazor Interactive Server 单体全栈能力，以及高度可扩展的命令行工具（`Abp.Spark.CLI`）与项目模板系统（`Abp.Spark.Templates`）。

---

## 目录
- [架构概览](#架构概览)
  - [1. 解决方案与项目组织](#1-解决方案与项目组织)
  - [2. 7层 DDD 单体架构 (src/)](#2-7层-ddd-单体架构-src)
  - [3. CLI 与模板架构 (Spark.sln / Spark.slnx)](#3-cli-与模板架构-sparksln--sparkslnx)
- [环境依赖](#环境依赖)
- [CLI 工具与模板安装](#cli-工具与模板安装)
  - [1. 编译并打包本地模板与 CLI](#1-编译并打包本地模板与-cli)
  - [2. 安装 / 更新本地模板](#2-安装--更新本地模板)
  - [3. 安装 / 更新本地 CLI 工具](#3-安装--更新本地-cli-工具)
- [使用 CLI 创建新项目](#使用-cli-创建新项目)
  - [1. 基础命令语法](#1-基础命令语法)
  - [2. 参数说明](#2-参数说明)
  - [3. 命名一致性规则](#3-命名一致性规则)
  - [4. 创建示例](#4-创建示例)
- [使用 dotnet new 直接创建](#使用-dotnet-new-直接创建)
- [运行与开发](#运行与开发)
  - [1. 启动前端 Vite 资源（可选）](#1-启动前端-vite-资源可选)
  - [2. 数据库迁移与初始化](#2-数据库迁移与初始化)
  - [3. 运行 Web 项目](#3-运行-web-项目)
- [常用命令与维护](#常用命令与维护)

---

## 架构概览

### 1. 解决方案与项目组织
仓库采用双方案共存模式：
- **`Abp.Spark.sln`**：单体业务核心实现，包含 `src/` 下完整的 7 层 DDD 架构，作为模板制作的黄金基准源（Golden Master）。
- **`Spark.sln` / `Spark.slnx`**：开发工具生态，包含 CLI 工具、模板打包工程、基础支持库与单元测试。

```
Abp-Spark-framework/
├── src/                                  # 7层 DDD + Blazor 单体架构源码 (Abp.Spark.sln)
│   ├── Abp.Spark.Domain.Shared/          # 常量、枚举、错误码与通用契约
│   ├── Abp.Spark.Domain/                 # 领域实体、聚合根、领域事件与数据播种
│   ├── Abp.Spark.Application.Contracts/  # 应用层接口、DTO、权限定义
│   ├── Abp.Spark.Application/            # 应用服务实现、AutoMapper 配置
│   ├── Abp.Spark.EntityFrameworkCore/    # EF Core 仓储、DbContext、多数据库驱动与迁移
│   ├── Abp.Spark.HttpApi/                # Web API 控制器与 HTTP 暴露
│   └── Abp.Spark.Web/                    # Blazor Interactive Server + Vite/Tailwind 宿主
│
├── Abp.Spark.Console/                    # Spark CLI 工具 (PackageId: Abp.Spark.CLI, 命令: spark)
├── Abp.Spark.Templates/                  # 模板管理包 (PackageId: Abp.Spark.Templates)
│   └── working/
│       ├── templates/
│       │   └── Spark.Templates.Blazor/   # 核心脚手架模板 (shortName: sparkblazor)
│       └── Abp.Spark.Templates.csproj
├── Abp.Spark.Library/                    # CLI 核心共享逻辑库
└── Abp.Spark.Tests/                      # 单元测试工程
```

### 2. 7层 DDD 单体架构 (src/)
- **多数据库自由切换**：在 `Abp.Spark.EntityFrameworkCore` 与 `Abp.Spark.Web/appsettings.json` 中原生支持：
  - `SqlServer`（默认）
  - `PostgreSql`
  - `MySql`
  - `Sqlite`
- **Blazor Interactive Server + 前端现代化生态**：
  - 内置 Vite 5 + Tailwind CSS + DaisyUI 组件库构建流水线；
  - 生产环境发布自包含静态编译资产，开发环境支持快速样式热更。
- **开箱即用企业级功能**：
  - 基于 Lion.AbpPro 的组织架构、用户管理、角色权限控制、数据字典、国际化语言管理等企业功能。

### 3. CLI 与模板架构 (Spark.sln / Spark.slnx)
- **`spark` CLI**：基于 `McMaster.Extensions.CommandLineUtils` 开发的全局命令行工具，提供项目创建、代码生成及辅助开发能力。
- **`sparkblazor` 模板**：采用 .NET Templating 引擎规范，利用 `join` 与 `switch` 生成器，实现 CustomerName、ProjectName 与数据库连接字符串的动态替换与文件系统级命名重写。

---

## 环境依赖
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) (或更高版本)
- [Node.js](https://nodejs.org/) (v18+，仅用于修改前端 Tailwind/Vite 样式时)
- [.NET EF Core CLI 工具](https://learn.microsoft.com/en-us/ef/core/cli/)：
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## CLI 工具与模板安装

### 1. 编译并打包本地模板与 CLI
在仓库根目录下执行构建与打包命令，输出 nupkg 存放在本地 `nupkgs` 目录：

```bash
# 1. 打包 Templates
cd "Abp.Spark.Templates/working"
dotnet pack -c Release -o ../../../nupkgs/

# 2. 打包 CLI
cd "../../Abp.Spark.Console"
dotnet pack -c Release -o ../../nupkgs/
```

### 2. 安装 / 更新本地模板
```bash
# 卸载旧版模板（若已有安装）
dotnet new uninstall Abp.Spark.Templates

# 安装新生成的本地模板包
dotnet new install "<本地绝对路径>/nupkgs/Abp.Spark.Templates.2.0.0.nupkg"

# 查看安装状态，确认已包含 sparkblazor
dotnet new list sparkblazor
```

### 3. 安装 / 更新本地 CLI 工具
```bash
# 卸载旧版本
dotnet tool uninstall --global Abp.Spark.CLI

# 安装新生成的 CLI 全局工具
dotnet tool install --global --add-source "<本地绝对路径>/nupkgs" Abp.Spark.CLI

# 验证 CLI 命令是否生效
spark --help
spark new --help
```

---

## 使用 CLI 创建新项目

### 1. 基础命令语法
```bash
spark new <ProjectName> [options]
```

### 2. 参数说明
| 参数 | 缩写 | 默认值 | 说明 |
| :--- | :--- | :--- | :--- |
| `<ProjectName>` | 无 | *必填* | 项目主名称（例如：`Mall`, `Order`, `Wms`） |
| `--customer` | `-c` | `Abp` | 客户/组织命名空间前缀（例如：`Acme`, `Flex`） |
| `--db` | `-d` | `SqlServer` | 数据库类型：`SqlServer`, `PostgreSql`, `MySql`, `Sqlite` |
| `--type` | `-t` | `blazor` | 项目模板类型（目前主推 `blazor`） |

### 3. 命名一致性规则
通过 `spark new` 创建的项目将实现 **4 层命名绝对一致**：
1. **解决方案名**：`<Customer>.<Project>.slnx`
2. **所有工程目录名**：
   - `<Customer>.<Project>.Application`
   - `<Customer>.<Project>.Application.Contracts`
   - `<Customer>.<Project>.Domain`
   - `<Customer>.<Project>.Domain.Shared`
   - `<Customer>.<Project>.EntityFrameworkCore`
   - `<Customer>.<Project>.HttpApi`
   - `<Customer>.<Project>.Web`
3. **工程文件名与引用**：`<Customer>.<Project>.xxx.csproj` 及对应 `ProjectReference`
4. **内部代码与类名**：
   - 命名空间全部更新为 `<Customer>.<Project>.*`
   - 模块与关键类前缀更新为紧凑驼峰 `<Customer><Project>WebModule`、`<Customer><Project>DbContext`

### 4. 创建示例

#### 示例 A：创建基于 PostgreSQL 的商城后台项目
```bash
spark new Mall -c Flex -d postgres
```
生成后目录结构如下：
```
Mall/
├── Flex.Mall.slnx
├── Flex.Mall.Application/
├── Flex.Mall.Application.Contracts/
├── Flex.Mall.Domain/
├── Flex.Mall.Domain.Shared/
├── Flex.Mall.EntityFrameworkCore/
├── Flex.Mall.HttpApi/
└── Flex.Mall.Web/
```
生成的 `appsettings.json` 会自动配置：
```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=FlexMallDb;Username=postgres;Password=your_password"
  },
  "Database": {
    "Provider": "PostgreSql"
  }
}
```

#### 示例 B：创建基于 SQLite 的独立轻量应用
```bash
spark new Crm -c Acme -d sqlite
```

#### 示例 C：创建默认 SQL Server 企业单体应用
```bash
spark new Erp -c Global -d mssql
```

---

## 使用 dotnet new 直接创建
除了使用 `spark new` 命令行，也可以直接通过 .NET 原生 CLI 调用本模板：

```bash
dotnet new sparkblazor \
  -o MyProject \
  --CustomerName Acme \
  --ProjectName Shop \
  --DbProvider PostgreSql
```

---

## 运行与开发

### 1. 启动前端 Vite 资源（可选）
模板内置了已编译好的前端静态文件（`wwwroot/build/assets/`），可直接启动后端。若需要深度定制前端 CSS 样式或 UI 组件：
```bash
cd <Customer>.<Project>.Web
npm install
npm run build      # 编译 CSS / JS 到 wwwroot
# 或
npm run dev        # 启动热更新服务 (Port: 5173)
```

### 2. 数据库迁移与初始化
根据项目选用的数据库（如 PostgreSQL / MySQL / SQL Server），在启动前确认连接字符串并执行数据库迁移：
```bash
cd <Customer>.<Project>.EntityFrameworkCore
dotnet ef database update --startup-project ../<Customer>.<Project>.Web
```
*(注：若使用 SQLite，系统启动时将通过 `EnsureCreated()` 自动初始化数据库)*

### 3. 运行 Web 项目
```bash
cd <Customer>.<Project>.Web
dotnet run
```
浏览器访问控制台提示的地址（如 `https://localhost:5001` 或 `http://localhost:5000`）：
- **Swagger API 文档**：`https://localhost:5001/swagger`
- **Blazor 管理后台**：`https://localhost:5001`（默认管理员账号：`admin` / 密码依种子数据设置）

---

## 常用命令与维护

| 目标操作 | 对应命令 |
| :--- | :--- |
| 本地更新模板包 | `dotnet pack Abp.Spark.Templates/working/ -c Release -o ./nupkgs/ && dotnet new install ./nupkgs/Abp.Spark.Templates.2.0.0.nupkg` |
| 本地更新 CLI 工具 | `dotnet pack Abp.Spark.Console/ -c Release -o ./nupkgs/ && dotnet tool update -g --add-source ./nupkgs Abp.Spark.CLI` |
| 查看 CLI 版本 | `spark --version` |
| 查看 CLI 帮助 | `spark new --help` |
| 列出已安装模板 | `dotnet new list` |
| 卸载本地模板 | `dotnet new uninstall Abp.Spark.Templates` |
| 卸载全局 CLI | `dotnet tool uninstall --global Abp.Spark.CLI` |

---

## 许可证
基于 [MIT License](LICENSE.txt) 开源。
