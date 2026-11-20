using Volo.Abp.Application.Services;

namespace Abp.Spark.Menus;

public interface IMenuAppService : IApplicationService
{
    Task<List<MenuDto>> GetUserMenusAsync();
    Task<List<MenuDto>> GetAllAsync();
    Task<MenuDto> GetAsync(Guid id);
    Task<MenuDto> CreateAsync(CreateUpdateMenuDto input);
    Task<MenuDto> UpdateAsync(Guid id, CreateUpdateMenuDto input);
    Task DeleteAsync(Guid id);
}

public class CreateUpdateMenuDto
{
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public SidebarItemType Type { get; set; } = SidebarItemType.Link;
    public int SortOrder { get; set; }
    public string? PermissionName { get; set; }
    public bool IsActive { get; set; } = true;
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
