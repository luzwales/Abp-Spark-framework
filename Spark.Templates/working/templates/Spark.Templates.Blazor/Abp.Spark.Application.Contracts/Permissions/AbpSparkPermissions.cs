namespace Abp.Spark.Permissions;

public static class AbpSparkPermissions
{
    public const string SystemGroupName = "SparkSystem";

    public static class Menus
    {
        public const string Default = SystemGroupName + ".Menus";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }

    public const string GroupName = "SparkManufacturing";

    public static class Customers
    {
        public const string Default = GroupName + ".Customers";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string ManageStatus = Default + ".ManageStatus";
    }
}
