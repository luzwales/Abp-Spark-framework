using Volo.Abp.Application.Services;

namespace Abp.Spark.Dashboard;

public interface IDashboardAppService : IApplicationService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}

public class DashboardSummaryDto
{
    public int TotalUsers { get; set; }
    public int ActiveWorkOrders { get; set; }
    public double SystemOeePercentage { get; set; }
    public List<KpiCardDto> Cards { get; set; } = new();
}

public class KpiCardDto
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;
    public string Icon { get; set; } = "Sparkles";
}
