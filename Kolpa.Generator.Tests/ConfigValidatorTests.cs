using Kolpa.Generator.Models;
using Kolpa.Generator.Services;
using Xunit;

namespace Kolpa.Generator.Tests;

public class ConfigValidatorTests
{
    private static ConfigValidator CreateValidator(string projectRoot)
    {
        return new ConfigValidator(new PhysicalFileSystem(), projectRoot);
    }

    [Fact]
    public void Validate_UnknownHighlightingProvider_ReportsErrorMd001()
    {
        var root = TestHelpers.CreateTempProject("{}");
        var config = TestHelpers.ConfigFromJson(
            """{"markdown":{"highlighting":{"provider":"vscode"}}}"""
        );

        var issues = CreateValidator(root).Validate(config);

        Assert.Contains(issues, i => i.Code == "MD001" && i.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Validate_UnknownImageProcessor_ReportsErrorImg001()
    {
        var root = TestHelpers.CreateTempProject("{}");
        var config = TestHelpers.ConfigFromJson("""{"assets":{"images":{"processor":"gimp"}}}""");

        var issues = CreateValidator(root).Validate(config);

        Assert.Contains(issues, i => i.Code == "IMG001" && i.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Validate_EmptySiteUrl_ReportsWarningSite002()
    {
        var root = TestHelpers.CreateTempProject("{}");
        var config = TestHelpers.ConfigFromJson("""{"site":{"url":""}}""");

        var issues = CreateValidator(root).Validate(config);

        Assert.Contains(issues, i => i.Code == "SITE002");
    }

    [Fact]
    public void Validate_UnknownHighlightingTheme_ReportsWarningMd002()
    {
        var root = TestHelpers.CreateTempProject("{}");
        var config = TestHelpers.ConfigFromJson(
            """{"markdown":{"highlighting":{"enabled":true,"theme":"solarized"}}}"""
        );

        var issues = CreateValidator(root).Validate(config);

        Assert.Contains(issues, i => i.Code == "MD002");
    }

    [Fact]
    public void Validate_AssetFingerprintWithoutManifest_ReportsWarningAsst001()
    {
        var root = TestHelpers.CreateTempProject("{}");
        var config = TestHelpers.ConfigFromJson(
            """{"assets":{"processing":{"enabled":true,"fingerprint":true,"manifestFile":""}}}"""
        );

        var issues = CreateValidator(root).Validate(config);

        Assert.Contains(issues, i => i.Code == "ASST001");
    }

    [Fact]
    public void Validate_AssetFingerprintWithInvalidHashLength_ReportsWarningAsst002()
    {
        var root = TestHelpers.CreateTempProject("{}");
        var config = TestHelpers.ConfigFromJson(
            """{"assets":{"processing":{"enabled":true,"fingerprint":true,"hashLength":0}}}"""
        );

        var issues = CreateValidator(root).Validate(config);

        Assert.Contains(issues, i => i.Code == "ASST002");
    }

    [Fact]
    public void Validate_MissingIo_PathsOff_ReportsNoErrors()
    {
        var root = TestHelpers.CreateTempProject("{}");
        var config = TestHelpers.ConfigFromJson(
            """{"paths":{"output":"dist","pages":"pages","layouts":"layouts"}}"""
        );

        var issues = CreateValidator(root).Validate(config);

        Assert.DoesNotContain(issues, i => i.Severity == DiagnosticSeverity.Error);
    }

    [Fact]
    public void Validate_CustomThemeInMap_NoWarningMd002()
    {
        var root = TestHelpers.CreateTempProject("{}");
        var config = TestHelpers.ConfigFromJson(
            """{"markdown":{"highlighting":{"enabled":true,"theme":"solarized","customTheme":{"solarized":"#002b36"}}}}"""
        );

        var issues = CreateValidator(root).Validate(config);

        Assert.DoesNotContain(issues, i => i.Code == "MD002");
    }
}
