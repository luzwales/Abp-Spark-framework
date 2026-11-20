using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace Abp.Spark.OrganizationUnits;

public interface IOrganizationUnitAppService : IApplicationService
{
    Task<List<OrganizationUnitTreeDto>> GetTreeAsync();
    Task<OrganizationUnitDto> GetAsync(Guid id);
    Task<OrganizationUnitDto> CreateAsync(CreateOrganizationUnitInput input);
    Task<OrganizationUnitDto> UpdateAsync(UpdateOrganizationUnitInput input);
    Task DeleteAsync(Guid id);
    Task AddRoleToOrganizationUnitAsync(AddRoleToOrganizationUnitInput input);
    Task AddRoleToChildrenOrganizationUnitAsync(AddRoleToChildrenOrganizationUnitInput input);
    Task AddRoleToManagerAsync(AddRoleToManagerInput input);
    Task AddRoleToChildrenManagersAsync(AddRoleToChildrenManagersInput input);
    Task<List<IdentityRoleDto>> GetRolesAsync(Guid organizationUnitId);
}
