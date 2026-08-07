# Kolpa Static Site Generator - Documentation

Kolpa Engine features a custom, Astro-like Static Site Generator built entirely in modern C# (.NET 10). It utilizes Fluid (Liquid templates) and Markdig (Markdown compiling) to create extensible, data-driven, and highly optimized static websites.

---

## Directory Structure

An ordinary Kolpa website project utilizes the following folder structure:

```text
ProjectRoot/
├── config.json          # Main configuration file
├── layouts/             # Page shell layout templates (.liquid)
│   └── default.liquid   # Default wrapper layout
├── pages/               # Main website pages (.liquid, .html)
│   └── index.liquid     # Home page
├── content/             # Markdown content collections (directories)
│   └── blog/            # Blog collection posts
│       └── first.md
├── data/                # Global static JSON data mappings
│   └── projects.json
├── assets/              # Static styling, scripting, and media files
│   ├── styles/
│   └── features/
└── dist/                # Output directory generated during build (output)
```

---

## Configuration (`config.json`)

Configure your static content compiler using a root `config.json` file:

```json
{
  "site": {
    "title": "Kolpa Engine",
    "description": "A modular, hackable open-source game engine."
  },
  "paths": {
    "pages": "pages",
    "layouts": "layouts",
    "content": "content",
    "data": "data",
    "assets": "assets",
    "output": "dist"
  },
  "collections": {
    "blog": {
      "source": "content/blog",
      "pattern": "*.md",
      "output": "/blog/{slug}/"
    },
    "docs": {
      "source": "content/docs",
      "pattern": "*.md",
      "output": "/docs/{slug}/"
    }
  },
  "seo": {
    "robots": {
      "enabled": true,
      "output": "robots.txt",
      "sitemap": true,
      "rules": []
    },
    "jsonLd": { "enabled": true, "type": "WebSite", "image": "" }
  }
}
```

> Full optional sections — `rss`, `atom`, `json`, `seo`, `markdown`, `assets`, `cache` —
> are documented below.

### Markdown Rendering (`markdown`)

The Markdown pipeline is assembled from configurable extensions. Everything is opt-in so
the same generator can serve simple or advanced Markdown across projects.

```json
{
  "markdown": {
    "extensions": {
      "advanced": true,
      "tables": true,
      "taskLists": true,
      "footnotes": true,
      "autoIdentifiers": true,
      "strikethrough": true,
      "autoLinks": true,
      "definitionLists": false,
      "emojiSmiles": false,
      "mathematics": false
    },
    "highlighting": {
      "enabled": true,
      "provider": "builtin",
      "theme": "dark",
      "cssPrefix": "hl-",
      "generateCss": true,
      "cssFile": "highlight.css",
      "customTheme": {}
    }
  }
}
```

- `extensions.advanced` enables Markdig's full advanced set as a baseline; individual flags
  layer more features on top (tables, task lists, footnotes, auto identifiers, etc.).
- `highlighting.provider` is `builtin` (class-based tokenizer) or `passthrough` (no colors).
- `highlighting.theme` is `light`, `dark`, or a custom name defined via `customTheme`
  (token name → color, e.g. `{ "keyword": "#ff7b72" }`).
- Code highlighting always emits CSS classes (e.g. `hl-keyword`) — never inline styles. When
  `generateCss` is true a reusable stylesheet is written to the output assets folder, which
  you link from your layout (e.g. `<link rel="stylesheet" href="/assets/highlight.css">`).

Fenced code blocks keep their language identifier and get a `highlighted` marker:

```html
<pre
  class="hl-theme-dark"
><code class="language-csharp highlighted">...</code></pre>
```

### Image Processing (`assets.images`)

Raster images are automatically detected, optimized, and converted into responsive WebP
variants. Originals are preserved by default so existing markup keeps working.

```json
{
  "assets": {
    "images": {
      "enabled": true,
      "processor": "imagesharp",
      "optimize": true,
      "generateWebP": true,
      "generateAvif": false,
      "quality": 85,
      "maxWidth": 1920,
      "preserveOriginal": true,
      "sizes": [320, 640, 1280, 1920],
      "include": ["png", "jpg", "jpeg", "webp"]
    }
  }
}
```

- `processor` is `imagesharp` or `passthrough`. The generator never depends on a specific
  image library at the call site — only via the configured processor.
- Images are never upscaled: a 640px source produces only smaller variants.
- Processed metadata is exposed to templates keyed by relative asset path:

```
{{ images['features/editor.png'].src }}      -> /assets/features/editor.webp
{{ images['features/editor.png'].width }}
{{ images['features/editor.png'].height }}
{{ images['features/editor.png'].sources }}  -> list of { src, width, format }
```

Example responsive markup:

```html
<picture>
  <source
    type="image/webp"
    srcset="
    {% for s in images['features/editor.png'].sources %}{{ s.src }} {{ s.width }}w{% unless forloop.last %},{% endunless %}{% endfor %}"
    sizes="100vw"
  />
  <img src="{{ images['features/editor.png'].src }}" alt="..." />
</picture>
```

### Caching (`cache`)

Incremental builds skip reprocessing unchanged files using content-addressed caches.

```json
{
  "cache": {
    "enabled": true,
    "directory": ".generator-cache"
  }
}
```

- Rendered Markdown and processed images are keyed by content hash plus a configuration
  signature, so editing a file (or its settings) correctly invalidates the cache.
- The cache directory is `.gitignore`d by default and cleared by `kolpa clean`.

### Search Engine Optimization (`seo`, `rss`, `atom`, `json`)

The generator emits SEO artifacts automatically: `robots.txt`, an RSS 2.0 feed, an Atom feed,
a JSON Feed, and per-page JSON-LD structured data. All are configuration-driven and available
to any site.

```json
{
  "rss": {
    "enabled": true,
    "collection": "blog",
    "output": "feed.xml",
    "link": "/blog/"
  },
  "atom": {
    "enabled": true,
    "collection": "blog",
    "output": "atom.xml",
    "link": "/blog/"
  },
  "json": {
    "enabled": true,
    "collection": "blog",
    "output": "feed.json",
    "link": "/blog/"
  },
  "seo": {
    "robots": {
      "enabled": true,
      "output": "robots.txt",
      "sitemap": true,
      "rules": ["Disallow: /private/"]
    },
    "jsonLd": {
      "enabled": true,
      "type": "WebSite",
      "image": ""
    }
  }
}
```

- Each feed (`feed.xml`, `atom.xml`, `feed.json`) publishes a configured collection. Set
  `enabled: false` for formats you do not need.
- `rss`/`atom`/`json` require `site.url` to be set; otherwise the feed is skipped (you will see
  a `SITE002` warning).
- `seo.robots` writes `robots.txt` with a `Sitemap:` reference when `sitemap` is true.
  `rules` holds explicit directives appended verbatim.
- `seo.jsonLd` injects a `<script type="application/ld+json">` block into the `<head>` of every
  generated HTML page. `type` mirrors Schema.org (`WebSite`, `Organization`, `Blog`, etc.).
  The default `image` falls back to `/assets/icon.png` unless you set one.

### Build Pipeline

Stages run in this order, each implementing `IBuildStage`:

```text
Load Configuration -> Discover Files -> Load Content -> Process Markdown ->
Highlight Code -> Load Data -> Build Collections -> Resolve Routes ->
Build Tag Archives -> Build Navigation -> Generate Metadata -> Render Templates ->
Live Reload Injection -> Write Output -> Process Images -> Optimize Assets ->
Run Post-Build
```

Images and assets are optimized after the output folder is written so a clean rebuild never
deletes them.

---

## Publishing a Standalone Binary

The generator publishes as a **single-file, self-contained, compressed executable** — no
`.NET` SDK or runtime needed on the target machine. This is ideal for running in CI (GitHub
Pages/Actions), Termux/Android, or directly in VS Code's terminal.

The project defaults to single-file publishing (`PublishSingleFile`, `SelfContained`,
`IncludeNativeLibrariesForSelfExtract`, `EnableCompressionInSingleFile`), so any
`dotnet publish -r <rid>` produces one binary.

### Scripts

```bash
# bash (Linux / macOS / Termux)
bash Kolpa.Generator/scripts/publish.sh            # all platforms
bash Kolpa.Generator/scripts/publish.sh linux-x64  # one platform

# PowerShell (Windows)
powershell -ExecutionPolicy Bypass -File Kolpa.Generator\scripts\publish.ps1 win-x64
```

Output goes to `bin/<rid>/`:

| Runtime ID    | Target                                         |
| ------------- | ---------------------------------------------- |
| `win-x64`     | Windows x64 (`.exe`)                           |
| `linux-x64`   | Linux x64 (desktop, server, CI)                |
| `linux-arm64` | Linux ARM64 (Raspberry Pi, **Termux/Android**) |
| `osx-x64`     | macOS Intel                                    |
| `osx-arm64`   | macOS Apple Silicon                            |

### Running the Binary

The executable takes the same commands as the `dotnet run` CLI, and uses the current
directory as the project root (pass `--dir <path>` to target another folder):

```bash
./bin/linux-x64/Kolpa.Generator build
./bin/linux-x64/Kolpa.Generator doctor --dir ./my-site
./bin/win-x64/Kolpa.Generator.exe serve --port 8080
```

In Termux, use the `linux-arm64` binary (`chmod +x` it first). Because it is self-contained,
you do not need `dotnet` installed on the device.

### GitHub Actions

A workflow (`.github/workflows/publish.yml`) builds and smoke-tests all five RIDs on every
push to `main` and uploads them as build artifacts. You can also trigger it manually via
**Actions → Kolpa Generator CLI → Run workflow**.

---

## CLI Reference

Run commands inside your project root directory using the `dotnet` CLI:

### 1. Build the Website

Generates static pages and copies assets into the target output directory:

```bash
dotnet run --project Kolpa.Generator -- build [options]
```

**Options:**

- `--verbose`, `-v`: Output detailed compilation steps, link checks, and file generation paths.
- `--watch`, `-w`: Start a file watcher. Automatically recompiles and updates your site when pages, layouts, data, or collections change.

### 2. Run Local Development Server

Launches a preview server at `http://localhost:5000/` and hosts your compiled website:

```bash
dotnet run --project Kolpa.Generator -- serve [options]
```

**Options:**

- `--port`, `-p <number>`: Override the default port (e.g. `--port 8080`).
- `--watch`, `-w`: Runs the file watcher concurrently to live-reload changes while serving.

### 3. Clean Project Files

Deletes the compiled output folder to reset the state:

```bash
dotnet run --project Kolpa.Generator -- clean
```

`clean` also wipes the `.generator-cache` directory when caching is enabled.

### 4. Validate Configuration (`doctor`)

Checks `config.json` and the project layout, then reports findings with stable error codes
and a colored summary. Exits with code `0` when no errors are found, `1` if any are:

```bash
dotnet run --project Kolpa.Generator -- doctor [--dir <path>]
```

Example output:

```text
Kolpa SSG Doctor
------------------------------------------------------------
[ERROR] MD001: Unknown highlighting provider 'vscode'. Allowed: builtin, passthrough.
[WARN ] PATH002: Source folder 'pages' does not exist: ./pages
[WARN ] SITE002: 'site.url' is empty. RSS feed and sitemap generation require an absolute URL.
------------------------------------------------------------
Doctor found 3 finding(s): 1 error(s), 2 warning(s).
```

Validation also runs automatically during every build; findings are reported with the same
codes so config problems are never silent.

---

## Layouts and Page Templates

Every page and markdown content document can declare layout inheritance using YAML frontmatter metadata:

### Page Example (`pages/index.liquid`)

```yaml
---
layout: default
title: Home Page
description: Welcome to Kolpa Engine.
---
<h1>{{ page.title }}</h1>
<p>{{ page.description }}</p>
```

The rendering engine processes this page and inserts it as the `{{ content }}` or `{{ page.content }}` token inside `layouts/default.liquid`:

### Layout Nesting

Layouts may declare their own parent via YAML frontmatter, so you can wrap a content layout inside the site shell. A template that needs its own chrome can inherit `default`:

```liquid
---
layout: default
---
<article class="post">
  <h1>{{ page.title }}</h1>
  {{ content }}
</article>
```

The engine renders the inner layout first, then passes its output as `{{ content }}` to the parent layout automatically.

### Layout Example (`layouts/default.liquid`)

```liquid
<!doctype html>
<html>
<head>
  <title>{{ site.title }} - {{ page.title }}</title>
  <link rel="stylesheet" href="/assets/styles/kolpa.css">
</head>
<body>
  <main>
    {{ content }}
  </main>
</body>
</html>
```

---

## Content Collections

Group content documents inside `content/<collection-name>/` folders and declare them under `"collections"` in `config.json`.

Access your collection lists inside templates using the `collections.<name>` loops. Each item exposes its metadata, `content`, `slug`, and the resolved clean `url` (use this for links). Posts are ordered newest-first by their `date`:

```liquid
{% for post in collections.blog %}
  <article>
    <h2><a href="{{ post.url }}">{{ post.title }}</a></h2>
    <time>{{ post.date | date_format: "MMMM d, yyyy" }}</time>
    <p>{{ post.description }}</p>
  </article>
{% endfor %}
```

### Blog Example

Declare the collection in `config.json`, drop Markdown posts (with `layout: post`) into `content/blog/`, and a clean URL like `/blog/my-post/` is generated:

```json
"collections": {
  "blog": {
    "source": "content/blog",
    "pattern": "*.md",
    "output": "/blog/{slug}/"
  }
}
```

---

## Global JSON Data

Save JSON files inside the `data/` folder. They are parsed and exposed under the `data.<filename>` variables dynamically:

### Data File (`data/projects.json`)

```json
[{ "name": "Kolpa Engine", "language": "C#" }]
```

### Usage

```liquid
<ul>
  {% for project in data.projects %}
    <li>{{ project.name }} ({{ project.language }})</li>
  {% endfor %}
</ul>
```

---

## Template Extension Filters

The engine registers custom extension filters for utility methods:

### Date Formatting (`date_format`)

Format raw dates inside templates:

```liquid
<span>{{ post.date | date_format: "dd MMM yyyy" }}</span>
```

### Limit Array Size (`limit`)

Constrain loops sizes (e.g. limiting to the 3 latest posts):

```liquid
{% assign latest = collections.blog | limit: 3 %}
{% for post in latest %}
  ...
{% endfor %}
```

---

## Extension Plugins & Custom Stages

Kolpa Engine supports custom plugins to tap into the build lifecycle. Plugins implement the `IEnginePlugin` interface and register custom service dependencies, route decorators, or post-build steps.

### Plugin Definition Example

Plugins can define build-time tasks by implementing `IBuildStep`:

```csharp
public class SitemapPlugin : IEnginePlugin, IBuildStep
{
    public string Name => "Sitemap Generator Plugin";

    public void ConfigureServices(IServiceCollection services, GeneratorConfig config)
    {
        // Register this instance as a post-build step sitemapper
        services.AddSingleton<IBuildStep>(this);
    }

    public async Task ExecuteAsync(SiteContext siteContext, string outputDir)
    {
        var sitemapFile = Path.Combine(outputDir, "sitemap.xml");
        var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>...";
        await File.WriteAllTextAsync(sitemapFile, xml);
    }
}
```

### Plugin Registration

Plugins are loaded in the dependency injection container inside `Program.cs`:

```csharp
var plugins = new List<IEnginePlugin>
{
    new CoreEnginePlugin(projectDir, configPath),
    new SitemapPlugin(),
    new RssPlugin(),
    new SeoPlugin(),
};
```

The generator ships with three built-in post-build plugins out of the box:

- `SitemapPlugin` — writes `sitemap.xml` from the site's resolved routes.
- `RssPlugin` — writes `feed.xml` from the configured collection.
- `SeoPlugin` — writes `robots.txt`, `atom.xml`, and `feed.json`, and injects JSON-LD
  structured data into every generated page.
