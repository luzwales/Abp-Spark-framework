using Abp.Spark.Users;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Abp.Spark.Dashboard;

public class DashboardAppService : ApplicationService, IDashboardAppService
{
    private readonly IRepository<AppUser, Guid> _userRepository;

    public DashboardAppService(IRepository<AppUser, Guid> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var userCount = (int)await _userRepository.GetCountAsync();

        return new DashboardSummaryDto
        {
            TotalUsers = userCount,
            ActiveWorkOrders = 128,
            SystemOeePercentage = 87.5,
            Cards = new List<KpiCardDto>
            {
                new() { Title = "注册用户总数", Value = $"{userCount} 人", Badge = "+12% 环比", Icon = "Users" },
                new() { Title = "制造在制品工单 (WIP)", Value = "128 单", Badge = "正常排产", Icon = "Boxes" },
                new() { Title = "车间综合效率 (OEE)", Value = "87.5%", Badge = "达到目标", Icon = "Activity" },
                new() { Title = "系统服务状态", Value = "100% 正常", Badge = "ABP vNext", Icon = "Server" }
            }
        };
    }
}
