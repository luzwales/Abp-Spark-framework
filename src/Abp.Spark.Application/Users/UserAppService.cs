using System.Security.Cryptography;
using System.Text;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Abp.Spark.Users;

public class UserAppService : ApplicationService, IUserAppService
{
    private readonly IRepository<AppUser, Guid> _userRepository;

    public UserAppService(IRepository<AppUser, Guid> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> GetCurrentAsync()
    {
        var users = await _userRepository.GetListAsync();
        var user = users.FirstOrDefault();
        if (user == null)
        {
            throw new UserFriendlyException("未找到当前用户");
        }
        return ObjectMapper.Map<AppUser, UserDto>(user);
    }

    public async Task<UserDto> LoginAsync(LoginInputDto input)
    {
        var hash = HashPassword(input.Password);
        var user = await _userRepository.FirstOrDefaultAsync(u => u.Email == input.Email && u.PasswordHash == hash);
        if (user == null)
        {
            throw new UserFriendlyException("账号或密码错误");
        }
        return ObjectMapper.Map<AppUser, UserDto>(user);
    }

    public async Task<UserDto> RegisterAsync(CreateUserDto input)
    {
        var exists = await _userRepository.AnyAsync(u => u.Email == input.Email);
        if (exists)
        {
            throw new UserFriendlyException("该邮箱已被注册");
        }

        var hash = HashPassword(input.Password);
        var user = new AppUser(GuidGenerator.Create(), input.Name, input.Email, hash);
        await _userRepository.InsertAsync(user, autoSave: true);
        return ObjectMapper.Map<AppUser, UserDto>(user);
    }

    public async Task<List<UserDto>> GetListAsync()
    {
        var users = await _userRepository.GetListAsync();
        return ObjectMapper.Map<List<AppUser>, List<UserDto>>(users);
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
