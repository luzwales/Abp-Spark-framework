using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Abp.Spark.Customers;

public interface ICustomerAppService : IApplicationService
{
    Task<PagedResultDto<CustomerDto>> GetListAsync(GetCustomerListInput input);
    Task<CustomerDto> GetAsync(Guid id);
    Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input);
    Task<CustomerDto> UpdateAsync(Guid id, CreateUpdateCustomerDto input);
    Task DeleteAsync(Guid id);
    Task ToggleStatusAsync(Guid id);
}
