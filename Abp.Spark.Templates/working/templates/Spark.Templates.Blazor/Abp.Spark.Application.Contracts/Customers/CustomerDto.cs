using Volo.Abp.Application.Dtos;

namespace Abp.Spark.Customers;

public class CustomerDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public string IndustryType { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string CreditLevel { get; set; } = "A";
    public string SettlementMethod { get; set; } = "月结30天";
    public bool IsActive { get; set; } = true;
    public string? Remark { get; set; }
}
