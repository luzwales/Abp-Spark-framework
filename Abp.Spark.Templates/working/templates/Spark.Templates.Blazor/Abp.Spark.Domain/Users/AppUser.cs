using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;

namespace Abp.Spark.Users;

public class AppUser : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Role { get; set; } = "User";

    [MaxLength(256)]
    public string? AvatarUrl { get; set; }

    public bool IsActive { get; set; } = true;

    protected AppUser()
    {
    }

    public AppUser(Guid id, string name, string email, string passwordHash, string role = "User")
        : base(id)
    {
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }
}
