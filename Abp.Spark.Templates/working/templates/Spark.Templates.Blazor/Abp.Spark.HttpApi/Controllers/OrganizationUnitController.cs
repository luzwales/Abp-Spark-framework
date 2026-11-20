using Abp.Spark.OrganizationUnits;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Identity;

namespace Abp.Spark.Controllers;

[Tags("OrganizationUnits")]
[Route("api/organization-units")]
public class OrganizationUnitController : AbpControllerBase, IOrganizationUnitAppService
{
    private readonly IOrganizationUnitAppService _organizationUnitAppService;

    public OrganizationUnitController(IOrganizationUnitAppService organizationUnitAppService)
    {
        _organizationUnitAppService = organizationUnitAppService;
    }

    [HttpGet("tree")]
    public Task<List<OrganizationUnitTreeDto>> GetTreeAsync() => _organizationUnitAppService.GetTreeAsync();

    [HttpGet("{id}")]
    public Task<OrganizationUnitDto> GetAsync(Guid id) => _organizationUnitAppService.GetAsync(id);

    [HttpPost]
    public Task<OrganizationUnitDto> CreateAsync([FromBody] CreateOrganizationUnitInput input) => _organizationUnitAppService.CreateAsync(input);

    [HttpPut]
    public Task<OrganizationUnitDto> UpdateAsync([FromBody] UpdateOrganizationUnitInput input) => _organizationUnitAppService.UpdateAsync(input);

    [HttpDelete("{id}")]
    public Task DeleteAsync(Guid id) => _organizationUnitAppService.DeleteAsync(id);

    [HttpPost("add-role")]
    public Task AddRoleToOrganizationUnitAsync([FromBody] AddRoleToOrganizationUnitInput input) => _organizationUnitAppService.AddRoleToOrganizationUnitAsync(input);

    [HttpPost("add-role-to-children")]
    public Task AddRoleToChildrenOrganizationUnitAsync([FromBody] AddRoleToChildrenOrganizationUnitInput input) => _organizationUnitAppService.AddRoleToChildrenOrganizationUnitAsync(input);

    [HttpPost("add-role-to-manager")]
    public Task AddRoleToManagerAsync([FromBody] AddRoleToManagerInput input) => _organizationUnitAppService.AddRoleToManagerAsync(input);

    [HttpPost("add-role-to-children-managers")]
    public Task AddRoleToChildrenManagersAsync([FromBody] AddRoleToChildrenManagersInput input) => _organizationUnitAppService.AddRoleToChildrenManagersAsync(input);

    [HttpGet("{organizationUnitId}/roles")]
    public Task<List<IdentityRoleDto>> GetRolesAsync(Guid organizationUnitId) => _organizationUnitAppService.GetRolesAsync(organizationUnitId);
}
