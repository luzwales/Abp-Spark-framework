using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;

namespace Abp.Spark;

[DependsOn(
    typeof(AbpValidationModule),
    typeof(AbpLocalizationModule)
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
