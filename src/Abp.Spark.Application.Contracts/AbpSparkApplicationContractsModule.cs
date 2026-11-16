using Lion.AbpPro.BasicManagement;
using Lion.AbpPro.DataDictionaryManagement;
using Lion.AbpPro.LanguageManagement;
using Lion.AbpPro.NotificationManagement;
using Volo.Abp.Application;
using Volo.Abp.Modularity;

namespace Abp.Spark;

[DependsOn(
    typeof(AbpSparkDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(BasicManagementApplicationContractsModule),
    typeof(NotificationManagementApplicationContractsModule),
    typeof(DataDictionaryManagementApplicationContractsModule),
    typeof(LanguageManagementApplicationContractsModule)
)]
public class AbpSparkApplicationContractsModule : AbpModule
{
}
