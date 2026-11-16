using System.Security.Cryptography;
using System.Text;
using Abp.Spark.Users;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;

namespace Abp.Spark;

public class AbpSparkDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<AppUser, Guid> _userRepository;
    private readonly IGuidGenerator _guidGenerator;

    public AbpSparkDataSeedContributor(
        IRepository<AppUser, Guid> userRepository,
        IGuidGenerator guidGenerator)
    {
        _userRepository = userRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _userRepository.GetCountAsync() > 0)
        {
            return;
        }

        var adminUser = new AppUser(
            _guidGenerator.Create(),
            "Andrew Lu",
            "andrew@example.com",
            HashPassword("Password123!"),
            role: "Admin"
        );

        await _userRepository.InsertAsync(adminUser, autoSave: true);
    }

    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
