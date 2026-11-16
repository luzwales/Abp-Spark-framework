using Abp.Spark.Domain.Menus;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Abp.Spark.Menus;

public class MenuAppService : ApplicationService, IMenuAppService
{
    private readonly IRepository<MenuConfig, Guid> _menuRepository;

    public MenuAppService(IRepository<MenuConfig, Guid> menuRepository)
    {
        _menuRepository = menuRepository;
    }

    public async Task<List<MenuDto>> GetUserMenusAsync()
    {
        var menus = await _menuRepository.GetListAsync(m => m.IsActive);
        return BuildTree(menus);
    }

    public async Task<List<MenuDto>> GetAllAsync()
    {
        var menus = await _menuRepository.GetListAsync();
        return BuildTree(menus);
    }

    private static List<MenuDto> BuildTree(List<MenuConfig> allMenus)
    {
        var rootMenus = allMenus
            .Where(m => m.ParentId == null)
            .OrderBy(m => m.SortOrder)
            .Select(MapToDto)
            .ToList();

        foreach (var root in rootMenus)
        {
            FillChildren(root, allMenus);
        }

        return rootMenus;
    }

    private static void FillChildren(MenuDto parent, List<MenuConfig> allMenus)
    {
        parent.Children = allMenus
            .Where(m => m.ParentId == parent.Id)
            .OrderBy(m => m.SortOrder)
            .Select(MapToDto)
            .ToList();

        foreach (var child in parent.Children)
        {
            FillChildren(child, allMenus);
        }
    }

    private static MenuDto MapToDto(MenuConfig m) => new()
    {
        Id = m.Id,
        ParentId = m.ParentId,
        Name = m.Name,
        Label = m.Label,
        Icon = m.Icon,
        Route = m.Route,
        Type = m.Type,
        SortOrder = m.SortOrder,
        PermissionName = m.PermissionName,
        IsActive = m.IsActive
    };
}
