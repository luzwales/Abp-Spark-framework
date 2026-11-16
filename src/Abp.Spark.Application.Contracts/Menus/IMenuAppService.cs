using Volo.Abp.Application.Services;

namespace Abp.Spark.Menus;

public interface IMenuAppService : IApplicationService
{
    Task<List<MenuDto>> GetUserMenusAsync();
    Task<List<MenuDto>> GetAllAsync();
}

public class MenuDto
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public SidebarItemType Type { get; set; }
    public int SortOrder { get; set; }
    public string? PermissionName { get; set; }
    public bool IsActive { get; set; }
    public List<MenuDto> Children { get; set; } = new();
}
