using Abp.Spark.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;

namespace Abp.Spark.Controllers;

[Tags("User")]
[Route("api/users")]
[IgnoreAntiforgeryToken]
public class UserController : AbpControllerBase, IUserAppService
{
    private readonly IUserAppService _userAppService;

    public UserController(IUserAppService userAppService)
    {
        _userAppService = userAppService;
    }

    [HttpGet("current")]
    public Task<UserDto> GetCurrentAsync() => _userAppService.GetCurrentAsync();

    [HttpPost("login")]
    public Task<UserDto> LoginAsync([FromBody] LoginInputDto input) => _userAppService.LoginAsync(input);

    [HttpPost("register")]
    public Task<UserDto> RegisterAsync([FromBody] CreateUserDto input) => _userAppService.RegisterAsync(input);

    [HttpGet]
    public Task<List<UserDto>> GetListAsync() => _userAppService.GetListAsync();

    [HttpPost]
    public Task<UserDto> CreateAsync([FromBody] CreateUserDto input) => _userAppService.CreateAsync(input);

    [HttpPut("{id}")]
    public Task<UserDto> UpdateAsync(Guid id, [FromBody] UpdateUserDto input) => _userAppService.UpdateAsync(id, input);

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id) => _userAppService.DeleteAsync(id);
}
