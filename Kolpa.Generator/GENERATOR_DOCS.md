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

Access your collection lists inside templates using the `collections.<name>` loops:

```liquid
{% for post in collections.blog %}
  <article>
    <h2><a href="{{ post.slug }}">{{ post.title }}</a></h2>
    <p>{{ post.description }}</p>
  </article>
{% endfor %}
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
