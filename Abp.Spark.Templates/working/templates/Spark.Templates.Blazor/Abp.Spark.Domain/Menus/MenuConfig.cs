using System.ComponentModel.DataAnnotations.Schema;
using Abp.Spark.Menus;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Abp.Spark.Domain.Menus;

public class MenuConfig : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public SidebarItemType Type { get; set; } = SidebarItemType.Link;
    public int SortOrder { get; set; }
    public string? PermissionName { get; set; }
    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(ParentId))]
    public virtual MenuConfig? Parent { get; set; }
    public virtual ICollection<MenuConfig> Children { get; set; } = new List<MenuConfig>();
    public virtual ICollection<MenuRoleVisibility> RoleVisibilities { get; set; } = new List<MenuRoleVisibility>();

    protected MenuConfig() { }

    public MenuConfig(
        Guid id,
        Guid? parentId,
        string name,
        string label,
        string? icon,
        string? route,
        SidebarItemType type,
        int sortOrder,
        string? permissionName = null,
        bool isActive = true)
        : base(id)
    {
        ParentId = parentId;
        Name = name;
        Label = label;
        Icon = icon;
        Route = route;
        Type = type;
        SortOrder = sortOrder;
        PermissionName = permissionName;
        IsActive = isActive;
    }
}
