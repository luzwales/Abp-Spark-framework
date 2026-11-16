using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace Abp.Spark.Domain.Menus;

public class MenuRoleVisibility : CreationAuditedEntity<Guid>, ISoftDelete
{
    public Guid MenuConfigId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

    [ForeignKey(nameof(MenuConfigId))]
    public virtual MenuConfig? MenuConfig { get; set; }

    protected MenuRoleVisibility() { }

    public MenuRoleVisibility(Guid id, Guid menuConfigId, string roleName)
        : base(id)
    {
        MenuConfigId = menuConfigId;
        RoleName = roleName;
    }
}
