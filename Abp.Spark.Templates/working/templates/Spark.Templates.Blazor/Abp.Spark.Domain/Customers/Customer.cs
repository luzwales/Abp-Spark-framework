using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Abp.Spark.Customers;

/// <summary>
/// 制造业务客户聚合根实体 (支持多租户与全审计软删除)
/// </summary>
public class Customer : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>
    /// 客户编码 (ERP/MES 业务流水号)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 客户全称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 客户简称
    /// </summary>
    public string ShortName { get; set; } = string.Empty;

    /// <summary>
    /// 统一社会信用代码 / 税号
    /// </summary>
    public string TaxNumber { get; set; } = string.Empty;

    /// <summary>
    /// 所属行业分类 (如：新能源、汽车电子、消费电子、半导体装备)
    /// </summary>
    public string IndustryType { get; set; } = string.Empty;

    /// <summary>
    /// 质量与商务联系人
    /// </summary>
    public string ContactPerson { get; set; } = string.Empty;

    /// <summary>
    /// 联系电话
    /// </summary>
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>
    /// 电子邮箱
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// 客户信用评级 (AAA, AA, A, B, C)
    /// </summary>
    public string CreditLevel { get; set; } = "A";

    /// <summary>
    /// 商务账期/结算方式 (如：月结30天, 月结60天, 款到发货)
    /// </summary>
    public string SettlementMethod { get; set; } = "月结30天";

    /// <summary>
    /// 客户状态 (True=正常合作, False=冻结停用)
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 备注说明
    /// </summary>
    public string? Remark { get; set; }

    protected Customer() { }

    public Customer(
        Guid id,
        string code,
        string name,
        string shortName,
        string taxNumber,
        string industryType,
        string contactPerson,
        string contactPhone,
        string? contactEmail = null,
        string creditLevel = "A",
        string settlementMethod = "月结30天",
        bool isActive = true,
        string? remark = null,
        Guid? tenantId = null) : base(id)
    {
        Code = code;
        Name = name;
        ShortName = shortName;
        TaxNumber = taxNumber;
        IndustryType = industryType;
        ContactPerson = contactPerson;
        ContactPhone = contactPhone;
        ContactEmail = contactEmail;
        CreditLevel = creditLevel;
        SettlementMethod = settlementMethod;
        IsActive = isActive;
        Remark = remark;
        TenantId = tenantId;
    }
}
