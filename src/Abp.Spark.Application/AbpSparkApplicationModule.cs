using Lion.AbpPro.BasicManagement;
using Lion.AbpPro.DataDictionaryManagement;
using Lion.AbpPro.LanguageManagement;
using Lion.AbpPro.NotificationManagement;
using Volo.Abp.Application;
using Volo.Abp.AutoMapper;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.Modularity;

namespace Abp.Spark;

[DependsOn(
    typeof(AbpSparkDomainModule),
    typeof(AbpSparkApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpAutoMapperModule),
    typeof(AbpBlobStoringFileSystemModule),
    typeof(BasicManagementApplicationModule),
    typeof(NotificationManagementApplicationModule),
    typeof(DataDictionaryManagementApplicationModule),
    typeof(LanguageManagementApplicationModule)
)]
public class AbpSparkApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<AbpSparkApplicationModule>();
        });
    }
}
