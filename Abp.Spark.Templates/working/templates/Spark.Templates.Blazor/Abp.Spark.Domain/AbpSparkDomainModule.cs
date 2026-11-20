using Lion.AbpPro.BasicManagement;
using Lion.AbpPro.DataDictionaryManagement;
using Lion.AbpPro.LanguageManagement;
using Lion.AbpPro.NotificationManagement;
using Volo.Abp.BlobStoring;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Abp.Spark;

[DependsOn(
    typeof(AbpSparkDomainSharedModule),
    typeof(AbpDddDomainModule),
    typeof(AbpBlobStoringModule),
    typeof(BasicManagementDomainModule),
    typeof(NotificationManagementDomainModule),
    typeof(DataDictionaryManagementDomainModule),
    typeof(LanguageManagementDomainModule)
)]
public class AbpSparkDomainModule : AbpModule
{
}
