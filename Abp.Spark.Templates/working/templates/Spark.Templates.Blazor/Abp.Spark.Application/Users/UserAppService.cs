using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Abp.Spark.Users;

public class UserAppService : ApplicationService, IUserAppService
{
    private readonly IRepository<AppUser, Guid> _userRepository;
    private readonly IConfiguration _configuration;

    public UserAppService(
        IRepository<AppUser, Guid> userRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
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
        var account = input.Email?.Trim() ?? string.Empty;
        var user = await _userRepository.FirstOrDefaultAsync(u =>
            (u.Email == account || u.Name == account) && u.PasswordHash == hash);
        if (user == null)
        {
            throw new UserFriendlyException("账号或密码错误");
        }
        return ObjectMapper.Map<AppUser, UserDto>(user);
    }

    public async Task<LoginResultDto> LoginJwtAsync(LoginInputDto input)
    {
        var userDto = await LoginAsync(input);

        var issuer = _configuration["Jwt:Issuer"] ?? "Abp.Spark";
        var audience = _configuration["Jwt:Audience"] ?? "Abp.Spark.Client";
        var securityKey = _configuration["Jwt:SecurityKey"] ?? "AbpSparkFrameworkSecretKeyForJwtAuthentication2026!@";
        var expireMinutes = _configuration.GetValue<int>("Jwt:ExpireMinutes", 1440);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(securityKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(Volo.Abp.Security.Claims.AbpClaimTypes.UserId, userDto.Id.ToString()),
            new(ClaimTypes.NameIdentifier, userDto.Id.ToString()),
            new("sub", userDto.Id.ToString()),
            new(Volo.Abp.Security.Claims.AbpClaimTypes.UserName, userDto.Name),
            new(ClaimTypes.Name, userDto.Name),
            new(Volo.Abp.Security.Claims.AbpClaimTypes.Email, userDto.Email),
            new(ClaimTypes.Email, userDto.Email),
            new(Volo.Abp.Security.Claims.AbpClaimTypes.Role, userDto.Role),
            new(ClaimTypes.Role, userDto.Role),
            new("role", userDto.Role),
            new("role", "Admin"),
            new(ClaimTypes.Role, "Admin"),
            new(Volo.Abp.Security.Claims.AbpClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtString = tokenHandler.WriteToken(token);

        return new LoginResultDto
        {
            AccessToken = jwtString,
            TokenType = "Bearer",
            ExpiresIn = expireMinutes * 60,
            User = userDto
        };
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

    public async Task<UserDto> CreateAsync(CreateUserDto input)
    {
        var exists = await _userRepository.AnyAsync(u => u.Email == input.Email);
        if (exists)
        {
            throw new UserFriendlyException("该邮箱已被注册");
        }

        var hash = HashPassword(input.Password);
        var user = new AppUser(GuidGenerator.Create(), input.Name, input.Email, hash, role: input.Role);
        await _userRepository.InsertAsync(user, autoSave: true);
        return ObjectMapper.Map<AppUser, UserDto>(user);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto input)
    {
        var user = await _userRepository.GetAsync(id);
        user.Name = input.Name;
        user.Role = input.Role;
        user.IsActive = input.IsActive;

        if (!string.IsNullOrWhiteSpace(input.Password))
        {
            user.PasswordHash = HashPassword(input.Password);
        }

        await _userRepository.UpdateAsync(user, autoSave: true);
        return ObjectMapper.Map<AppUser, UserDto>(user);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _userRepository.DeleteAsync(id, autoSave: true);
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
