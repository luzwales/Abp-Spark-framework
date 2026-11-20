using Volo.Abp.Application.Dtos;

namespace Abp.Spark.OrganizationUnits;

public class OrganizationUnitDto : ExtensibleAuditedEntityDto<Guid>
{
    public Guid? ParentId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public int? ManagerLevel { get; set; }
}

public class OrganizationUnitTreeDto
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public int? ManagerLevel { get; set; }
    public List<OrganizationUnitTreeDto> Children { get; set; } = new();
}

public class CreateOrganizationUnitInput
{
    public string DisplayName { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public Guid? ManagerId { get; set; }
    public int? ManagerLevel { get; set; }
}

public class UpdateOrganizationUnitInput
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public int? ManagerLevel { get; set; }
}

public class AddRoleToOrganizationUnitInput
{
    public Guid OrganizationUnitId { get; set; }
    public List<Guid> RoleIds { get; set; } = new();
}

public class AddRoleToChildrenOrganizationUnitInput
{
    public List<Guid> OrganizationUnitIds { get; set; } = new();
    public List<Guid> RoleIds { get; set; } = new();
}

public class AddRoleToManagerInput
{
    public List<Guid> OrganizationUnitIds { get; set; } = new();
    public List<Guid> RoleIds { get; set; } = new();
}

public class AddRoleToChildrenManagersInput
{
    public List<Guid> OrganizationUnitIds { get; set; } = new();
    public List<Guid> RoleIds { get; set; } = new();
}
