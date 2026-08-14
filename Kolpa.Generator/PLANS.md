# Static Site Generator (Idk what name to give this lol)

It is already modular, pipeline-driven, uses dependency injection, and is no longer coupled to the Kolpa website (Initially i made it to my kolpa website only, but it envolved more than just that). The remaining work focuses on making it production ready rather than endlessly refactoring to use on all my other projects.

ngl, I made this doc with ai slop, used a chat, not a agent.

---

# Phase 1 - Core Stabilization (Highest Priority)

Goal: Freeze the architecture and establish a stable foundation.

## Configuration

- Central configuration model
- Configuration validation
- Default values
- Environment overrides
- User-defined variables
- CLI overrides
- Configuration versioning
- Configuration schema documentation

## Diagnostics

- Error IDs
- Warning IDs
- Source locations
- Stack traces (verbose mode)
- Friendly error messages
- Build summary
- Performance summary
- Timing report

## Build Engine

- Incremental builds
- Build cache
- Dependency graph
- File hashing
- Change detection
- Parallel processing where safe
- Cancellation support
- Progress reporting

## CLI

Commands:

- build
- serve
- clean
- watch
- doctor
- init
- new
- validate
- plugins
- version

Options:

- --verbose
- --configuration
- --output
- --watch
- --production
- --development
- --clean
- --no-cache
- --threads

---

# Phase 2 - Content System

Goal: Support multiple content types.

## Parsers

- Markdown
- HTML
- MDX (future)
- Plain text
- Liquid
- Razor (optional)
- Custom parser plugins

## Frontmatter

- YAML
- TOML
- JSON
- Custom providers

## Content Types

- Pages
- Posts
- Documentation
- Changelog
- Tutorials
- Portfolio
- Projects
- Generic collections

## Metadata

- Tags
- Categories
- Authors
- Date
- Draft
- Slug
- Canonical URL
- Featured image
- Reading time

---

# Phase 3 - Template System

Goal: Production-ready rendering.

## Layouts

- Nested layouts
- Partial templates
- Components
- Includes
- Slots
- Sections

## Template Features

- Filters
- Functions
- Global variables
- Collection access
- Pagination helpers
- Navigation helpers
- Asset helpers

## Template Engines

- Fluid
- Scriban
- Razor
- Custom plugins

---

# Phase 4 - Asset Pipeline

Goal: Modern web asset processing.

## CSS

- SCSS
- Sass
- CSS minification
- Autoprefixer
- CSS bundling

## JavaScript

- Bundling
- Minification
- ES Modules
- TypeScript support

## Images

- Compression
- Resize
- Responsive images
- WebP generation
- AVIF generation
- Lazy-loading helpers

## Static Assets

- Fingerprinting
- Cache busting
- Compression
- Manifest generation

---

# Phase 5 - Routing

Goal: Flexible routing.

Support:

- Pretty URLs
- Slug generation
- Custom routes
- Dynamic routes
- Nested routes
- Redirects
- Aliases
- Route validation

---

# Phase 6 - Collections

Goal: Data-driven content.

Support:

- Generic collections
- Sorting
- Filtering
- Grouping
- Pagination
- Related content
- Previous/next
- Custom queries

---

# Phase 7 - Navigation

Support:

- Automatic navigation
- Breadcrumbs
- Sidebar trees
- Documentation navigation
- Menu ordering
- Hidden pages
- External links

---

# Phase 8 - Development Experience

## Watch Mode

- File watching
- Live reload
- Hot rebuild
- Incremental rebuild

## Development Server

- Static serving
- Custom port
- HTTPS
- Directory browsing (optional)
- SPA fallback

---

# Phase 9 - SEO

Support:

- Sitemap
- robots.txt
- RSS
- Atom
- JSON Feed
- Canonical URLs
- OpenGraph
- Twitter Cards
- Structured Data (JSON-LD)

---

# Phase 10 - Localization

Support:

- Multiple languages
- Translation files
- Localized routes
- Localized navigation
- Fallback language
- Language switcher

---

# Phase 11 - Plugin System

Goal: Make the engine extensible.

## Plugin Discovery

- Assembly loading
- Manifest
- Versioning
- Dependencies
- Enable/disable
- Configuration

## Extension Points

- Build stages
- Parsers
- Renderers
- Filters
- Functions
- Asset processors
- Route generators
- Diagnostics
- CLI commands

---

# Phase 12 - Data System

Support:

- JSON
- YAML
- TOML
- CSV
- XML
- SQLite (optional)
- REST API (optional)

---

# Phase 13 - Performance

Support:

- Parallel rendering
- Parallel parsing
- Memory pooling
- Streaming IO
- Cached templates
- Cached parsing
- Lazy loading

---

# Phase 14 - Quality

## Testing

- Unit tests
- Integration tests
- Snapshot tests
- Parser tests
- Routing tests
- Plugin tests

## Benchmarks

- Build time
- Memory usage
- Parallel scalability

---

# Phase 15 - Documentation

Create comprehensive documentation covering:

- Getting Started
- Installation
- Project Structure
- Configuration
- Content
- Templates
- Assets
- Plugins
- CLI
- Deployment
- API Reference
- Best Practices
- Migration Guide

---

# Phase 16 - Project Templates

Provide starter templates for common site types:

- Landing Page
- Personal Website
- Blog
- Documentation
- Portfolio
- Company Website
- Product Website
- Knowledge Base

---

# Phase 17 - Future Features (Optional)

- Visual theme marketplace
- Theme inheritance
- Incremental deployment
- Cloud builds
- Remote content providers
- Headless CMS integration
- GraphQL data layer
- Search index generation (Lunr, Pagefind, etc.)
- Built-in analytics helpers
- PWA support
- Offline caching
- Content previews
- Draft previews
- AI-assisted content generation hooks

---

# Definition of Done (v1.0)

The generator can be considered production-ready when it:

- Builds any static website through configuration alone.
- Has no website-specific logic in the core.
- Supports multiple template engines and content parsers.
- Provides a configurable, extensible pipeline.
- Offers incremental builds and a watch mode.
- Includes SEO essentials (sitemap, RSS, metadata).
- Handles modern asset processing.
- Exposes a documented plugin API.
- Has automated tests for core functionality.
- Is documented well enough for another developer to build a complete website without reading the source code (idk if anyone will do it anyways bruh).

At that point, I will stop expanding the architecture unless a real project exposes a concrete need. Future work should primarily be new features, performance improvements (important asf), plugins, and bug fixes rather than additional layers of abstraction.
