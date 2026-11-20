using System.Diagnostics;

namespace Abp.Spark.Tests.Cli;

[TestClass]
public class CreateProjectCliTests
{
    private static readonly string TempDir = Path.Combine(Path.GetTempPath(), "AbpSparkCliTests", Guid.NewGuid().ToString("N"));

    [TestInitialize]
    public void Setup()
    {
        if (Directory.Exists(TempDir))
        {
            Directory.Delete(TempDir, true);
        }
        Directory.CreateDirectory(TempDir);
    }

    [TestCleanup]
    public void Cleanup()
    {
        try
        {
            if (Directory.Exists(TempDir))
            {
                Directory.Delete(TempDir, true);
            }
        }
        catch
        {
        }
    }

    [TestMethod]
    public void SparkNew_GeneratesProjectWithCustomerAndDb()
    {
        var outDir = Path.Combine(TempDir, "Billing");
        var psi = new ProcessStartInfo
        {
            FileName = "spark",
            Arguments = "new Billing -c Global -d sqlite",
            WorkingDirectory = TempDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        Assert.IsNotNull(process, "spark CLI should be available");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        Assert.AreEqual(0, process.ExitCode, $"spark new failed: {stderr}\n{stdout}");

        var slnxPath = Path.Combine(outDir, "Global.Billing.slnx");
        Assert.IsTrue(File.Exists(slnxPath), $"Solution {slnxPath} should exist");

        var webProj = Path.Combine(outDir, "Global.Billing.Web", "Global.Billing.Web.csproj");
        Assert.IsTrue(File.Exists(webProj), $"Web project {webProj} should exist");
    }
}
