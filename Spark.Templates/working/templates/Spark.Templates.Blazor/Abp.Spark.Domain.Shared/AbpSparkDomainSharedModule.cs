using Lion.AbpPro;
using Lion.AbpPro.Core;
using Lion.AbpPro.BasicManagement;
using Lion.AbpPro.DataDictionaryManagement;
using Lion.AbpPro.LanguageManagement;
using Lion.AbpPro.NotificationManagement;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;

namespace Abp.Spark;

[DependsOn(
    typeof(AbpValidationModule),
    typeof(AbpLocalizationModule),
    typeof(AbpProCoreModule),
    typeof(BasicManagementDomainSharedModule),
    typeof(NotificationManagementDomainSharedModule),
    typeof(DataDictionaryManagementDomainSharedModule),
    typeof(LanguageManagementDomainSharedModule)
)]
public class AbpSparkDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Languages.Add(new LanguageInfo("zh-Hans", "zh-Hans", "简体中文"));
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
        });
    }
}
