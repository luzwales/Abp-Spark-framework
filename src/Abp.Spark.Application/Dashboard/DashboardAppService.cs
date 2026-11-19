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
                new() { Title = "RegisteredUsers", Value = $"{userCount}", Badge = "Live", Icon = "Users" },
                new() { Title = "WipWorkOrders", Value = "128", Badge = "Online", Icon = "Boxes" },
                new() { Title = "WorkshopOee", Value = "87.5%", Badge = "TargetMet", Icon = "Activity" },
                new() { Title = "SystemStatus", Value = "100%", Badge = "Healthy", Icon = "Server" }
            }
        };
    }
}
