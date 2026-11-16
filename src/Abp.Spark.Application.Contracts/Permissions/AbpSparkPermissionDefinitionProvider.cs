using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Abp.Spark.Permissions;

public class AbpSparkPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var mfgGroup = context.AddGroup(
            AbpSparkPermissions.GroupName,
            new FixedLocalizableString("智能制造业务 (Manufacturing)")
        );

        var customers = mfgGroup.AddPermission(
            AbpSparkPermissions.Customers.Default,
            new FixedLocalizableString("客户档案 (Customers)"),
            multiTenancySide: MultiTenancySides.Both
        );

        customers.AddChild(
            AbpSparkPermissions.Customers.Create,
            new FixedLocalizableString("新增客户 (Create)"),
            multiTenancySide: MultiTenancySides.Both
        );

        customers.AddChild(
            AbpSparkPermissions.Customers.Update,
            new FixedLocalizableString("编辑客户 (Update)"),
            multiTenancySide: MultiTenancySides.Both
        );

        customers.AddChild(
            AbpSparkPermissions.Customers.Delete,
            new FixedLocalizableString("删除客户 (Delete)"),
            multiTenancySide: MultiTenancySides.Both
        );

        customers.AddChild(
            AbpSparkPermissions.Customers.ManageStatus,
            new FixedLocalizableString("合作状态管理 (ManageStatus)"),
            multiTenancySide: MultiTenancySides.Both
        );
    }
}
