using Abp.Spark.Domain.Menus;
using Microsoft.AspNetCore.Authorization;
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
        var activeMenus = await _menuRepository.GetListAsync(m => m.IsActive);
        var authorizedMenus = new List<MenuConfig>();

        foreach (var item in activeMenus)
        {
            if (string.IsNullOrWhiteSpace(item.PermissionName))
            {
                authorizedMenus.Add(item);
            }
            else
            {
                var isGranted = await AuthorizationService.IsGrantedAsync(item.PermissionName);
                if (isGranted)
                {
                    authorizedMenus.Add(item);
                }
            }
        }

        var tree = BuildTree(authorizedMenus);
        return tree.Where(t => t.Type != SidebarItemType.Group || t.Children.Any()).ToList();
    }

    public async Task<List<MenuDto>> GetAllAsync()
    {
        var menus = await _menuRepository.GetListAsync();
        return BuildTree(menus);
    }

    public async Task<MenuDto> GetAsync(Guid id)
    {
        var menu = await _menuRepository.GetAsync(id);
        return MapToDto(menu);
    }

    public async Task<MenuDto> CreateAsync(CreateUpdateMenuDto input)
    {
        var menu = new MenuConfig(
            GuidGenerator.Create(),
            input.ParentId,
            input.Name,
            input.Label,
            input.Icon,
            input.Route,
            input.Type,
            input.SortOrder,
            input.PermissionName,
            input.IsActive
        );
        await _menuRepository.InsertAsync(menu, autoSave: true);
        return MapToDto(menu);
    }

    public async Task<MenuDto> UpdateAsync(Guid id, CreateUpdateMenuDto input)
    {
        var menu = await _menuRepository.GetAsync(id);
        menu.ParentId = input.ParentId;
        menu.Name = input.Name;
        menu.Label = input.Label;
        menu.Icon = input.Icon;
        menu.Route = input.Route;
        menu.Type = input.Type;
        menu.SortOrder = input.SortOrder;
        menu.PermissionName = input.PermissionName;
        menu.IsActive = input.IsActive;

        await _menuRepository.UpdateAsync(menu, autoSave: true);
        return MapToDto(menu);
    }

    public async Task DeleteAsync(Guid id)
    {
        var children = await _menuRepository.GetListAsync(m => m.ParentId == id);
        foreach (var child in children)
        {
            await _menuRepository.DeleteAsync(child, autoSave: false);
        }
        await _menuRepository.DeleteAsync(id, autoSave: true);
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
