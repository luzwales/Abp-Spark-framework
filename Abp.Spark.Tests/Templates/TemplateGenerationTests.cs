using System.Diagnostics;

namespace Abp.Spark.Tests.Templates;

[TestClass]
public class TemplateGenerationTests
{
    private static readonly string TempDir = Path.Combine(Path.GetTempPath(), "AbpSparkTemplateTests", Guid.NewGuid().ToString("N"));

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
            // Ignore temp directory deletion errors
        }
    }

    [TestMethod]
    public void DotnetNew_SparkBlazor_GeneratesCorrectStructureAndNamespaces()
    {
        var outDir = Path.Combine(TempDir, "AcmeOrder");
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"new sparkblazor -o \"{outDir}\" --CustomerName Acme --ProjectName Order --DbProvider PostgreSql",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        Assert.IsNotNull(process, "Process should start successfully");
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        Assert.AreEqual(0, process.ExitCode, $"dotnet new failed: {stderr}\n{stdout}");

        // Verify solution file
        var slnxPath = Path.Combine(outDir, "Acme.Order.slnx");
        Assert.IsTrue(File.Exists(slnxPath), $"Solution file {slnxPath} should exist");

        // Verify folders
        string[] expectedProjects = 
        [
            "Acme.Order.Domain.Shared",
            "Acme.Order.Domain",
            "Acme.Order.Application.Contracts",
            "Acme.Order.Application",
            "Acme.Order.EntityFrameworkCore",
            "Acme.Order.HttpApi",
            "Acme.Order.Web"
        ];

        foreach (var proj in expectedProjects)
        {
            var projDir = Path.Combine(outDir, proj);
            Assert.IsTrue(Directory.Exists(projDir), $"Project directory {projDir} should exist");

            var csprojFile = Path.Combine(projDir, $"{proj}.csproj");
            Assert.IsTrue(File.Exists(csprojFile), $"Csproj {csprojFile} should exist");
        }

        // Verify file replacement: AcmeOrderApplicationModule.cs
        var appModule = Path.Combine(outDir, "Acme.Order.Application", "AcmeOrderApplicationModule.cs");
        Assert.IsTrue(File.Exists(appModule), $"{appModule} should exist");
        var appModuleText = File.ReadAllText(appModule);
        Assert.IsTrue(appModuleText.Contains("namespace Acme.Order;"), "Namespace should be Acme.Order");
        Assert.IsTrue(appModuleText.Contains("public class AcmeOrderApplicationModule"), "Class name should be AcmeOrderApplicationModule");

        // Verify DbProvider connection string in appsettings.json
        var appsettings = Path.Combine(outDir, "Acme.Order.Web", "appsettings.json");
        Assert.IsTrue(File.Exists(appsettings), $"{appsettings} should exist");
        var settingsText = File.ReadAllText(appsettings);
        Assert.IsTrue(settingsText.Contains("\"Provider\": \"PostgreSql\""), "Provider should be PostgreSql");
        Assert.IsTrue(settingsText.Contains("Host=localhost;Database=AbpSparkDb;Username=postgres;Password=your_password"), "PostgreSql connection string should be present");
    }
}
