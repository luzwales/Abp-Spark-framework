using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using Abp.Spark.Customers;

namespace Abp.Spark.Controllers;

[Authorize]
[Route("api/customers")]
[Tags("Customers")]
public class CustomerController : AbpController, ICustomerAppService
{
    private readonly ICustomerAppService _customerAppService;

    public CustomerController(ICustomerAppService customerAppService)
    {
        _customerAppService = customerAppService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<CustomerDto>> GetListAsync([FromQuery] GetCustomerListInput input)
    {
        return _customerAppService.GetListAsync(input);
    }

    [HttpGet("{id}")]
    public virtual Task<CustomerDto> GetAsync(Guid id)
    {
        return _customerAppService.GetAsync(id);
    }

    [HttpPost]
    public virtual Task<CustomerDto> CreateAsync([FromBody] CreateUpdateCustomerDto input)
    {
        return _customerAppService.CreateAsync(input);
    }

    [HttpPut("{id}")]
    public virtual Task<CustomerDto> UpdateAsync(Guid id, [FromBody] CreateUpdateCustomerDto input)
    {
        return _customerAppService.UpdateAsync(id, input);
    }

    [HttpDelete("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return _customerAppService.DeleteAsync(id);
    }

    [HttpPut("{id}/toggle-status")]
    public virtual Task ToggleStatusAsync(Guid id)
    {
        return _customerAppService.ToggleStatusAsync(id);
    }
}
