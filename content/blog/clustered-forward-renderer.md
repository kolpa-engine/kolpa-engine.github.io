---
layout: post
title: Inside the Clustered Forward Renderer
description: How Kolpa handles thousands of lights without collapsing into a fully deferred pipeline.
date: 2026-08-05
author: Gabriel Aplok
tags:
  - rendering
  - vulkan
  - d3d12
---

Kolpa renders with a clustered forward approach. Forward lets us keep MSAA, transparency sorting, and a simple mental model. Clustering gives us the light throughput we need.

## The Cluster Grid

Each frame the view frustum is divided into a 3D grid of clusters. Lights are binned into clusters based on bounds tests, then an index list is built so a shader can walk only the lights relevant to its tile.

A few thousand dynamic lights per frame is routine. Add baked lightmaps and light probes on top and the static scenes become almost free to light.

## The Pass Structure

1. Depth pre-pass fills the depth buffer
2. SSAO runs at half resolution and is combined
3. Shadow maps sample into the main pass
4. Forward pass shades with clustered light lists
5. TAA resolves the final image

```glsl
vec4 EvaluateUnlit(MaterialInput input)
{
    float rim = pow(1.0 - clamp(dot(
        normalize(input.Normal), input.ViewDir), 0.0, 1.0), 3.0);

    return vec4(input.BaseColor + rim * vec3(0.2, 1.0, 0.4), 1.0);
}
```

## Why Not Deferred

Deferred rendering couples the G-buffer layout to every future material. Clustered forward keeps the material system open: new shading code is just a shader, not a new G-buffer format. For an engine that wants hackability, that trade is worth making.
