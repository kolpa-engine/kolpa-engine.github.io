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
  }
}
```

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
};
```
