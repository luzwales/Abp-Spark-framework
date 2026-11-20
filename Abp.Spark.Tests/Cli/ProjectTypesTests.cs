using Spark.Console.Shared;

namespace Abp.Spark.Tests.Cli;

[TestClass]
public class ProjectTypesTests
{
    [TestMethod]
    public void IsValid_ReturnsTrue_ForBlazor()
    {
        Assert.IsTrue(ProjectTypes.IsValid("blazor"));
    }

    [TestMethod]
    public void IsValid_ReturnsFalse_ForNonBlazor()
    {
        Assert.IsFalse(ProjectTypes.IsValid("razor"));
        Assert.IsFalse(ProjectTypes.IsValid("mvc"));
        Assert.IsFalse(ProjectTypes.IsValid("api"));
        Assert.IsFalse(ProjectTypes.IsValid("unknown"));
    }

    [TestMethod]
    public void TranslateProjectTypeToTemplate_ReturnsSparkBlazor()
    {
        Assert.AreEqual("sparkblazor", ProjectTypes.TranslateProjectTypeToTemplate("blazor"));
        Assert.AreEqual("sparkblazor", ProjectTypes.TranslateProjectTypeToTemplate(null));
        Assert.AreEqual("sparkblazor", ProjectTypes.TranslateProjectTypeToTemplate("anything"));
    }
}
