---
layout: post
title: Writing Your First C# Script
description: A quick walkthrough of a gameplay component, from blank file to running in the engine.
date: 2026-08-03
author: Gabriel Aplok
image: /assets/features/hotreload.png
image_alt: Hot reload status overlay
tags:
  - csharp
  - scripting
  - tutorial
---

Gameplay logic in Kolpa can be written in C#. A script is a class that inherits from `MonoBehaviour` and gets updated every frame.

## The Basic Component

```csharp
using Kolpa.Engine;

public class Player : MonoBehaviour
{
    public float Speed = 5.0f;

    public override void OnUpdate(float dt)
    {
        var input = Input.GetAxis2D("MoveX", "MoveY");
        Transform.Translate(input * Speed * dt);
    }
}
```

Attach the component to an entity, press play, and you are done. The class is discovered by the assembly scanner and registered automatically.

## Hot Reload

Save the file and the engine recompiles the gameplay assembly without restarting the runtime. State on the component stays where it was, which is the whole point:

```csharp
public override void OnUpdate(float dt)
{
    // edit this, save, and watch it take effect live
    if (Input.GetKeyDown(KeyCode.Space))
        Velocity.Y = JumpForce;
}
```

## What You Get for Free

- Direct `.NET 10` runtime access with modern language features
- Unity-style `Input` class (GetKey, GetMouseButton, MouseDelta, CursorLocked)
- Blittable math structs (Vector2, Vector3) that cross the native boundary as-is
- A collectible `AssemblyLoadContext` so hot reload works without restarting

That is the entire onboarding. No codegen, no magic strings, no editor-side duplication.
