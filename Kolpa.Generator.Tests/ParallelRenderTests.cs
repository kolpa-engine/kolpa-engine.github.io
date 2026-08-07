using Kolpa.Generator.Config;
using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;
using Kolpa.Generator.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kolpa.Generator.Tests;

public class ParallelRenderTests
{
    private static TemplateService CreateTemplateService(string layoutsDir)
    {
        var renderer = new FluidTemplateRenderer(layoutsDir);
        var factory = new FluidTemplateContextFactory();
        return new TemplateService(renderer, factory, new NullLogger(), layoutsDir);
    }

    [Fact]
    public async Task Renders_Many_Routes_In_Parallel_Without_CrossContamination()
    {
        var layoutsDir = TestHelpers.TempDir();
        await File.WriteAllTextAsync(
            Path.Combine(layoutsDir, "default.liquid"),
            "<html>{{ page.title }}|{{ page.content }}</html>"
        );
        var service = CreateTemplateService(layoutsDir);

        var tasks = new List<Task<string>>();
        for (int i = 0; i < 32; i++)
        {
            // Each route gets its own context clone (as RenderTemplatesStage does) so the
            // concurrent renders never share mutable Page metadata.
            var routeCtx = new SiteContext
            {
                Site = new Dictionary<string, object> { ["title"] = "Site" },
            };
            var doc = new ContentDocument
            {
                Id = $"page-{i}",
                Slug = $"page-{i}",
                Body = "HELLO",
                Metadata = new ContentMetadata { Title = $"Title{i}", Layout = "default" },
            };
            tasks.Add(service.RenderPageAsync(doc, routeCtx));
        }

        var results = await Task.WhenAll(tasks);

        for (int i = 0; i < results.Length; i++)
        {
            Assert.Contains($"Title{i}", results[i]);
            Assert.Contains("HELLO", results[i]);
        }
    }
}
