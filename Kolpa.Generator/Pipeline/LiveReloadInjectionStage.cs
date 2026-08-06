using Kolpa.Generator.Interfaces;
using Kolpa.Generator.Models;

namespace Kolpa.Generator.Pipeline;

/// <summary>
/// Pipeline stage that injects an EventSource live-reload listener script into rendered HTML documents when watch mode is enabled.
/// </summary>
public class LiveReloadInjectionStage : IBuildStage
{
    public string Name => "Live Reload Script Injection";

    public Task ExecuteAsync(BuildContext context)
    {
        // Check if watch/serve mode is enabled in the context metadata
        if (
            context.Metadata.TryGetValue("WatchMode", out var watchVal)
            && watchVal is bool watchMode
            && watchMode
        )
        {
            context.AddDiagnostic(
                DiagnosticSeverity.Info,
                "Injecting live-reload EventSource scripts into HTML documents...",
                Name
            );

            var scriptToInject =
                "\n<script>\n"
                + "  (function() {\n"
                + "    const sse = new EventSource('/__livereload');\n"
                + "    sse.onmessage = (event) => {\n"
                + "      if (event.data === 'reload') {\n"
                + "        console.log('[LiveReload] Change detected, reloading page...');\n"
                + "        window.location.reload();\n"
                + "      }\n"
                + "    };\n"
                + "    sse.onerror = () => {\n"
                + "      console.warn('[LiveReload] Lost server connection. Retrying...');\n"
                + "    };\n"
                + "  })();\n"
                + "</script>\n"
                + "</body>";

            foreach (var route in context.Routes)
            {
                if (string.IsNullOrEmpty(route.RenderedHtml))
                    continue;

                var bodyIndex = route.RenderedHtml.LastIndexOf(
                    "</body>",
                    StringComparison.OrdinalIgnoreCase
                );
                if (bodyIndex != -1)
                {
                    route.RenderedHtml = string.Concat(
                        route.RenderedHtml.AsSpan(0, bodyIndex),
                        scriptToInject,
                        route.RenderedHtml.AsSpan(bodyIndex + 7)
                    );
                }
            }
        }

        return Task.CompletedTask;
    }
}
