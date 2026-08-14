---
layout: post
title: The Rendering Pipeline
description: How Kolpa renders 3D scenes with OpenGL and the roadmap for future renderer improvements.
date: 2026-08-05
author: Gabriel Aplok
image: /assets/features/materials.png
image_alt: PBR material rendering
tags:
  - rendering
  - opengl
  - roadmap
---

Kolpa currently renders with OpenGL through the kolpa C library (forked from raylib). The engine layer adds a render system that dispatches mesh drawing, materials, and shaders.

## Current State

The render system handles:
- Mesh loading and rendering via kolpa's `rlgl` abstraction
- PBR materials with albedo textures and custom shaders
- Skeletal animation on the GPU
- Instanced rendering for repeated objects
- Debug drawing (lines, rays, AABB, frustum)

The renderer is functional but basic. It uses OpenGL 3.3 on desktop and OpenGL ES 3.0 on mobile.

## What Is Planned

A custom clustered forward renderer is planned for a future version. The goals:

1. **Clustered light lists** -- bin lights into a 3D grid so each fragment only evaluates relevant lights
2. **Depth pre-pass** -- fill the depth buffer first to reduce overdraw
3. **Shadow mapping** -- cascaded shadow maps for directional lights
4. **SSAO** -- screen-space ambient occlusion at half resolution
5. **TAA** -- temporal anti-aliasing for smooth motion

This would replace the current rlgl-based renderer with a custom OpenGL 4.3 renderer that gives us full control over the pipeline.

## Why Not Vulkan or D3D12

OpenGL is simpler to ship and debug. A custom renderer on top of OpenGL 4.3 gives us compute shaders, SSBOs, and enough control for clustered lighting without the platform complexity of Vulkan or D3D12. If the engine needs them later, the backend-agnostic physics API shows the pattern: write a new backend file and change one link.

## Custom Shaders

The asset pipeline supports custom GLSL shaders. A `.glsl` file bakes to a versioned text blob, splits into vertex/fragment halves at import, and compiles via `LoadShaderFromMemory` at acquire time. Materials can reference a shader to override the default rendering.

```glsl
vec4 EvaluateUnlit(MaterialInput input)
{
    float rim = pow(1.0 - clamp(dot(
        normalize(input.Normal), input.ViewDir), 0.0, 1.0), 3.0);

    return vec4(input.BaseColor + rim * vec3(0.2, 1.0, 0.4), 1.0);
}
```

This is the current path. The clustered renderer is the next big rendering milestone.
