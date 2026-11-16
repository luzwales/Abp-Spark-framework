using Lion.AbpPro.BasicManagement;
using Lion.AbpPro.DataDictionaryManagement;
using Lion.AbpPro.LanguageManagement;
using Lion.AbpPro.NotificationManagement;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace Abp.Spark;

[DependsOn(
    typeof(AbpSparkApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule),
    typeof(BasicManagementHttpApiModule),
    typeof(NotificationManagementHttpApiModule),
    typeof(DataDictionaryManagementHttpApiModule),
    typeof(LanguageManagementHttpApiModule)
)]
public class AbpSparkHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(AbpSparkHttpApiModule).Assembly);
        });
    }
}
