using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Abp.Spark;

[DependsOn(
    typeof(AbpSparkDomainSharedModule),
    typeof(AbpDddDomainModule)
)]
public class AbpSparkDomainModule : AbpModule
{
}
