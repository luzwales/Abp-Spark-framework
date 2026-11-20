using System.Text.Json;

namespace Abp.Spark.Tests.Templates;

[TestClass]
public class TemplateConfigurationTests
{
    private static readonly string TemplateConfigPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "..", "..", "..", "..",
        "Abp.Spark.Templates", "working", "templates", "Spark.Templates.Blazor", ".template.config", "template.json");

    [TestMethod]
    public void TemplateJson_Exists()
    {
        Assert.IsTrue(File.Exists(TemplateConfigPath), $"template.json should exist at {TemplateConfigPath}");
    }

    [TestMethod]
    public void TemplateJson_HasRequiredSymbolsAndGenerators()
    {
        var jsonText = File.ReadAllText(TemplateConfigPath);
        using var doc = JsonDocument.Parse(jsonText);
        var root = doc.RootElement;

        Assert.AreEqual("sparkblazor", root.GetProperty("shortName").GetString());
        Assert.AreEqual("Abp.Spark Web App (ABP vNext + Blazor Monolith)", root.GetProperty("name").GetString());

        var symbols = root.GetProperty("symbols");
        Assert.IsTrue(symbols.TryGetProperty("CustomerName", out _), "Should have CustomerName symbol");
        Assert.IsTrue(symbols.TryGetProperty("ProjectName", out _), "Should have ProjectName symbol");
        Assert.IsTrue(symbols.TryGetProperty("DbProvider", out _), "Should have DbProvider symbol");
        Assert.IsTrue(symbols.TryGetProperty("FullNamespace", out var fullNs), "Should have FullNamespace symbol");
        Assert.IsTrue(symbols.TryGetProperty("CompactPrefix", out var compactPrefix), "Should have CompactPrefix symbol");
        Assert.IsTrue(symbols.TryGetProperty("DefaultConnection", out var defaultConn), "Should have DefaultConnection symbol");

        Assert.AreEqual("join", fullNs.GetProperty("generator").GetString());
        Assert.AreEqual("join", compactPrefix.GetProperty("generator").GetString());
        Assert.AreEqual("switch", defaultConn.GetProperty("generator").GetString());
    }

    [TestMethod]
    public void TemplateJson_HasDbProviderChoices()
    {
        var jsonText = File.ReadAllText(TemplateConfigPath);
        using var doc = JsonDocument.Parse(jsonText);
        var dbProvider = doc.RootElement.GetProperty("symbols").GetProperty("DbProvider");

        Assert.AreEqual("parameter", dbProvider.GetProperty("type").GetString());
        Assert.AreEqual("choice", dbProvider.GetProperty("datatype").GetString());

        var choices = dbProvider.GetProperty("choices");
        Assert.AreEqual(JsonValueKind.Array, choices.ValueKind);

        var choiceNames = new List<string>();
        foreach (var choice in choices.EnumerateArray())
        {
            choiceNames.Add(choice.GetProperty("choice").GetString()!);
        }

        CollectionAssert.Contains(choiceNames, "SqlServer");
        CollectionAssert.Contains(choiceNames, "PostgreSql");
        CollectionAssert.Contains(choiceNames, "MySql");
        CollectionAssert.Contains(choiceNames, "Sqlite");
    }
}
