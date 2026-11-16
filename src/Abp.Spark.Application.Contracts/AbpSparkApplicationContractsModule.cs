using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace Abp.Spark;

[DependsOn(
    typeof(AbpSparkDomainSharedModule),
    typeof(AbpDddApplicationContractsModule)
)]
public class AbpSparkApplicationContractsModule : AbpModule
{
}
