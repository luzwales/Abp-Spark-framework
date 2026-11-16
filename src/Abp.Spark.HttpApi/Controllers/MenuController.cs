using Abp.Spark.Menus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Abp.Spark.Controllers;

[Tags("Menu")]
[Route("api/menus")]
public class MenuController : AbpControllerBase, IMenuAppService
{
    private readonly IMenuAppService _menuAppService;

    public MenuController(IMenuAppService menuAppService)
    {
        _menuAppService = menuAppService;
    }

    [HttpGet("user-menus")]
    public Task<List<MenuDto>> GetUserMenusAsync() => _menuAppService.GetUserMenusAsync();

    [HttpGet("all")]
    public Task<List<MenuDto>> GetAllAsync() => _menuAppService.GetAllAsync();
}
