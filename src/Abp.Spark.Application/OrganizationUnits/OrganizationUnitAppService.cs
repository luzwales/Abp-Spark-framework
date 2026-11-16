using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using IdentityRole = Volo.Abp.Identity.IdentityRole;

namespace Abp.Spark.OrganizationUnits;

public class OrganizationUnitAppService : ApplicationService, IOrganizationUnitAppService
{
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly IOrganizationUnitRepository _organizationUnitRepository;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IdentityRoleManager _identityRoleManager;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;

    public OrganizationUnitAppService(
        OrganizationUnitManager organizationUnitManager,
        IOrganizationUnitRepository organizationUnitRepository,
        IdentityUserManager identityUserManager,
        IdentityRoleManager identityRoleManager,
        IRepository<IdentityRole, Guid> roleRepository)
    {
        _organizationUnitManager = organizationUnitManager;
        _organizationUnitRepository = organizationUnitRepository;
        _identityUserManager = identityUserManager;
        _identityRoleManager = identityRoleManager;
        _roleRepository = roleRepository;
    }

    public async Task<List<OrganizationUnitTreeDto>> GetTreeAsync()
    {
        var list = await _organizationUnitRepository.GetListAsync();
        var dtos = list.Select(ou =>
        {
            var dto = new OrganizationUnitDto
            {
                Id = ou.Id,
                ParentId = ou.ParentId,
                Code = ou.Code,
                DisplayName = ou.DisplayName
            };

            if (ou.ExtraProperties.TryGetValue("ManagerId", out var managerId) && managerId is Guid mId)
            {
                dto.ManagerId = mId;
            }
            if (ou.ExtraProperties.TryGetValue("ManagerName", out var managerName) && managerName is string mName)
            {
                dto.ManagerName = mName;
            }
            if (ou.ExtraProperties.TryGetValue("ManagerLevel", out var managerLevel) && managerLevel is int mLevel)
            {
                dto.ManagerLevel = mLevel;
            }

            return dto;
        }).ToList();

        return BuildTree(dtos, null);
    }

    public async Task<OrganizationUnitDto> GetAsync(Guid id)
    {
        var ou = await _organizationUnitRepository.GetAsync(id);
        var dto = new OrganizationUnitDto
        {
            Id = ou.Id,
            ParentId = ou.ParentId,
            Code = ou.Code,
            DisplayName = ou.DisplayName
        };

        if (ou.ExtraProperties.TryGetValue("ManagerId", out var managerId) && managerId is Guid mId)
        {
            dto.ManagerId = mId;
        }
        if (ou.ExtraProperties.TryGetValue("ManagerName", out var managerName) && managerName is string mName)
        {
            dto.ManagerName = mName;
        }
        if (ou.ExtraProperties.TryGetValue("ManagerLevel", out var managerLevel) && managerLevel is int mLevel)
        {
            dto.ManagerLevel = mLevel;
        }

        return dto;
    }

    public async Task<OrganizationUnitDto> CreateAsync(CreateOrganizationUnitInput input)
    {
        var ou = new OrganizationUnit(
            GuidGenerator.Create(),
            input.DisplayName,
            input.ParentId,
            CurrentTenant.Id
        );

        if (input.ManagerId.HasValue)
        {
            ou.ExtraProperties["ManagerId"] = input.ManagerId.Value;
            var user = await _identityUserManager.FindByIdAsync(input.ManagerId.Value.ToString());
            if (user != null)
            {
                ou.ExtraProperties["ManagerName"] = user.UserName ?? user.Name;
            }
            ou.ExtraProperties["ManagerLevel"] = input.ManagerLevel ?? 1;
        }

        await _organizationUnitManager.CreateAsync(ou);
        return new OrganizationUnitDto
        {
            Id = ou.Id,
            ParentId = ou.ParentId,
            Code = ou.Code,
            DisplayName = ou.DisplayName,
            ManagerId = input.ManagerId,
            ManagerName = ou.ExtraProperties.TryGetValue("ManagerName", out var mn) ? mn as string : null,
            ManagerLevel = input.ManagerLevel
        };
    }

    public async Task<OrganizationUnitDto> UpdateAsync(UpdateOrganizationUnitInput input)
    {
        var ou = await _organizationUnitRepository.GetAsync(input.Id);
        ou.DisplayName = input.DisplayName;

        if (input.ManagerId.HasValue)
        {
            ou.ExtraProperties["ManagerId"] = input.ManagerId.Value;
            var user = await _identityUserManager.FindByIdAsync(input.ManagerId.Value.ToString());
            if (user != null)
            {
                ou.ExtraProperties["ManagerName"] = user.UserName ?? user.Name;
            }
            ou.ExtraProperties["ManagerLevel"] = input.ManagerLevel ?? 1;
        }
        else
        {
            ou.ExtraProperties.Remove("ManagerId");
            ou.ExtraProperties.Remove("ManagerName");
            ou.ExtraProperties.Remove("ManagerLevel");
        }

        await _organizationUnitManager.UpdateAsync(ou);
        return await GetAsync(ou.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _organizationUnitManager.DeleteAsync(id);
    }

    public async Task AddRoleToOrganizationUnitAsync(AddRoleToOrganizationUnitInput input)
    {
        foreach (var roleId in input.RoleIds)
        {
            await _organizationUnitManager.AddRoleToOrganizationUnitAsync(roleId, input.OrganizationUnitId);
        }
    }

    public async Task AddRoleToChildrenOrganizationUnitAsync(AddRoleToChildrenOrganizationUnitInput input)
    {
        foreach (var ouId in input.OrganizationUnitIds)
        {
            foreach (var roleId in input.RoleIds)
            {
                await _organizationUnitManager.AddRoleToOrganizationUnitAsync(roleId, ouId);
            }

            var children = await _organizationUnitRepository.GetChildrenAsync(ouId);
            foreach (var child in children)
            {
                foreach (var roleId in input.RoleIds)
                {
                    await _organizationUnitManager.AddRoleToOrganizationUnitAsync(roleId, child.Id);
                }
            }
        }
    }

    public async Task AddRoleToManagerAsync(AddRoleToManagerInput input)
    {
        foreach (var ouId in input.OrganizationUnitIds)
        {
            var ou = await _organizationUnitRepository.GetAsync(ouId);
            if (ou.ExtraProperties.TryGetValue("ManagerId", out var managerIdObj) && managerIdObj is Guid managerId)
            {
                var user = await _identityUserManager.FindByIdAsync(managerId.ToString());
                if (user != null)
                {
                    foreach (var roleId in input.RoleIds)
                    {
                        var role = await _identityRoleManager.FindByIdAsync(roleId.ToString());
                        if (role != null && !await _identityUserManager.IsInRoleAsync(user, role.Name))
                        {
                            await _identityUserManager.AddToRoleAsync(user, role.Name);
                        }
                    }
                }
            }
        }
    }

    public async Task AddRoleToChildrenManagersAsync(AddRoleToChildrenManagersInput input)
    {
        foreach (var ouId in input.OrganizationUnitIds)
        {
            var children = await _organizationUnitRepository.GetChildrenAsync(ouId, includeDetails: true);
            foreach (var child in children)
            {
                if (child.ExtraProperties.TryGetValue("ManagerId", out var managerIdObj) && managerIdObj is Guid managerId)
                {
                    var user = await _identityUserManager.FindByIdAsync(managerId.ToString());
                    if (user != null)
                    {
                        foreach (var roleId in input.RoleIds)
                        {
                            var role = await _identityRoleManager.FindByIdAsync(roleId.ToString());
                            if (role != null && !await _identityUserManager.IsInRoleAsync(user, role.Name))
                            {
                                await _identityUserManager.AddToRoleAsync(user, role.Name);
                            }
                        }
                    }
                }
            }
        }
    }

    public async Task<List<IdentityRoleDto>> GetRolesAsync(Guid organizationUnitId)
    {
        var ou = await _organizationUnitRepository.GetAsync(organizationUnitId);
        var roles = await _organizationUnitRepository.GetRolesAsync(ou);
        return ObjectMapper.Map<List<IdentityRole>, List<IdentityRoleDto>>(roles);
    }

    private static List<OrganizationUnitTreeDto> BuildTree(List<OrganizationUnitDto> list, Guid? parentId)
    {
        var result = new List<OrganizationUnitTreeDto>();
        var children = list.Where(x => x.ParentId == parentId).OrderBy(x => x.Code).ToList();

        foreach (var item in children)
        {
            var node = new OrganizationUnitTreeDto
            {
                Id = item.Id,
                ParentId = item.ParentId,
                Code = item.Code,
                DisplayName = item.DisplayName,
                ManagerId = item.ManagerId,
                ManagerName = item.ManagerName,
                ManagerLevel = item.ManagerLevel,
                Children = BuildTree(list, item.Id)
            };
            result.Add(node);
        }

        return result;
    }
}
