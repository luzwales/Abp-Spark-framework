using Volo.Abp.Application.Dtos;

namespace Abp.Spark.Customers;

public class GetCustomerListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public string? IndustryType { get; set; }
    public bool? IsActive { get; set; }
}
