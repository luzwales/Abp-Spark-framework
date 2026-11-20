using System.ComponentModel.DataAnnotations;

namespace Abp.Spark.Customers;

public class CreateUpdateCustomerDto
{
    [Required(ErrorMessage = "客户编码不能为空")]
    [StringLength(64)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "客户全称不能为空")]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "客户简称不能为空")]
    [StringLength(64)]
    public string ShortName { get; set; } = string.Empty;

    [StringLength(64)]
    public string TaxNumber { get; set; } = string.Empty;

    [StringLength(64)]
    public string IndustryType { get; set; } = "智能制造";

    [Required(ErrorMessage = "商务/质量联系人不能为空")]
    [StringLength(64)]
    public string ContactPerson { get; set; } = string.Empty;

    [Required(ErrorMessage = "联系电话不能为空")]
    [StringLength(32)]
    public string ContactPhone { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "邮箱格式不正确")]
    [StringLength(128)]
    public string? ContactEmail { get; set; }

    [StringLength(16)]
    public string CreditLevel { get; set; } = "A";

    [StringLength(64)]
    public string SettlementMethod { get; set; } = "月结30天";

    public bool IsActive { get; set; } = true;

    [StringLength(512)]
    public string? Remark { get; set; }
}
