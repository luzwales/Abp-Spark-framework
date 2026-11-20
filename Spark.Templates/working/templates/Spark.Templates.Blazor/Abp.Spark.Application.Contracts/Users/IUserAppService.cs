using Volo.Abp.Application.Services;

namespace Abp.Spark.Users;

public interface IUserAppService : IApplicationService
{
    Task<UserDto> GetCurrentAsync();
    Task<UserDto> LoginAsync(LoginInputDto input);
    Task<LoginResultDto> LoginJwtAsync(LoginInputDto input);
    Task<UserDto> RegisterAsync(CreateUserDto input);
    Task<List<UserDto>> GetListAsync();
    Task<UserDto> CreateAsync(CreateUserDto input);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserDto input);
    Task DeleteAsync(Guid id);
}

public class UpdateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public string? Password { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsAuthenticated { get; set; }
}

public class CreateUserDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
}

public class LoginInputDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResultDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public UserDto User { get; set; } = default!;
}
