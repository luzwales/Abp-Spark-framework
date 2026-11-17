using Abp.Spark.Menus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Abp.Spark.Controllers;

[Tags("Menu")]
[Route("api/menus")]
[IgnoreAntiforgeryToken]
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

    [HttpGet("{id}")]
    public Task<MenuDto> GetAsync(Guid id) => _menuAppService.GetAsync(id);

    [HttpPost]
    public Task<MenuDto> CreateAsync([FromBody] CreateUpdateMenuDto input) => _menuAppService.CreateAsync(input);

    [HttpPut("{id}")]
    public Task<MenuDto> UpdateAsync(Guid id, [FromBody] CreateUpdateMenuDto input) => _menuAppService.UpdateAsync(id, input);

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id) => _menuAppService.DeleteAsync(id);
}
