using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Abp.Spark.Customers;

[Authorize]
public class CustomerAppService : ApplicationService, ICustomerAppService
{
    private readonly IRepository<Customer, Guid> _customerRepository;

    public CustomerAppService(IRepository<Customer, Guid> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<PagedResultDto<CustomerDto>> GetListAsync(GetCustomerListInput input)
    {
        var queryable = await _customerRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var filter = input.Filter.Trim().ToLower();
            queryable = queryable.Where(x =>
                x.Code.ToLower().Contains(filter) ||
                x.Name.ToLower().Contains(filter) ||
                x.ShortName.ToLower().Contains(filter) ||
                x.ContactPerson.ToLower().Contains(filter) ||
                x.ContactPhone.Contains(filter));
        }

        if (!string.IsNullOrWhiteSpace(input.IndustryType))
        {
            queryable = queryable.Where(x => x.IndustryType == input.IndustryType);
        }

        if (input.IsActive.HasValue)
        {
            queryable = queryable.Where(x => x.IsActive == input.IsActive.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
        );

        var dtos = items.Select(MapToDto).ToList();

        return new PagedResultDto<CustomerDto>(totalCount, dtos);
    }

    public async Task<CustomerDto> GetAsync(Guid id)
    {
        var entity = await _customerRepository.GetAsync(id);
        return MapToDto(entity);
    }

    public async Task<CustomerDto> CreateAsync(CreateUpdateCustomerDto input)
    {
        var entity = new Customer(
            GuidGenerator.Create(),
            code: input.Code.Trim(),
            name: input.Name.Trim(),
            shortName: input.ShortName.Trim(),
            taxNumber: input.TaxNumber?.Trim() ?? string.Empty,
            industryType: input.IndustryType?.Trim() ?? "智能制造",
            contactPerson: input.ContactPerson.Trim(),
            contactPhone: input.ContactPhone.Trim(),
            contactEmail: input.ContactEmail?.Trim(),
            creditLevel: input.CreditLevel ?? "A",
            settlementMethod: input.SettlementMethod ?? "月结30天",
            isActive: input.IsActive,
            remark: input.Remark?.Trim(),
            tenantId: CurrentTenant.Id
        );

        await _customerRepository.InsertAsync(entity, autoSave: true);
        return MapToDto(entity);
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, CreateUpdateCustomerDto input)
    {
        var entity = await _customerRepository.GetAsync(id);

        entity.Code = input.Code.Trim();
        entity.Name = input.Name.Trim();
        entity.ShortName = input.ShortName.Trim();
        entity.TaxNumber = input.TaxNumber?.Trim() ?? string.Empty;
        entity.IndustryType = input.IndustryType?.Trim() ?? "智能制造";
        entity.ContactPerson = input.ContactPerson.Trim();
        entity.ContactPhone = input.ContactPhone.Trim();
        entity.ContactEmail = input.ContactEmail?.Trim();
        entity.CreditLevel = input.CreditLevel ?? "A";
        entity.SettlementMethod = input.SettlementMethod ?? "月结30天";
        entity.IsActive = input.IsActive;
        entity.Remark = input.Remark?.Trim();

        await _customerRepository.UpdateAsync(entity, autoSave: true);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _customerRepository.DeleteAsync(id, autoSave: true);
    }

    public async Task ToggleStatusAsync(Guid id)
    {
        var entity = await _customerRepository.GetAsync(id);
        entity.IsActive = !entity.IsActive;
        await _customerRepository.UpdateAsync(entity, autoSave: true);
    }

    private static CustomerDto MapToDto(Customer entity)
    {
        return new CustomerDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            ShortName = entity.ShortName,
            TaxNumber = entity.TaxNumber,
            IndustryType = entity.IndustryType,
            ContactPerson = entity.ContactPerson,
            ContactPhone = entity.ContactPhone,
            ContactEmail = entity.ContactEmail,
            CreditLevel = entity.CreditLevel,
            SettlementMethod = entity.SettlementMethod,
            IsActive = entity.IsActive,
            Remark = entity.Remark,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }
}
