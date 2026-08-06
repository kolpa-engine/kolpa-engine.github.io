---
layout: post
title: Writing Your First C# Script
description: A quick walkthrough of a gameplay component, from blank file to running in the editor.
date: 2026-08-03
author: Gabriel Aplok
tags:
  - csharp
  - scripting
  - tutorial
---

Gameplay logic in Kolpa is written in C#. A script is just a class that inherits from `EntityComponent` and gets updated every frame.

## The Basic Component

```csharp
public class BasePlayer : EntityComponent
{
    public float Speed { get; set; } = 5.0f;

    public override void OnUpdate(float deltaTime)
    {
        Vector3 input = GameInput.GetMovementVector();
        Transform.Translate(input * Speed * deltaTime);
    }
}
```

Attach the component to an entity in the editor, press play, and you are done. The class is discovered by the assembly scanner and registered automatically.

## Hot Reload

Save the file and the editor recompiles the gameplay assembly without restarting the runtime. State on the component stays where it was, which is the whole point:

```csharp
public override void OnUpdate(float deltaTime)
{
    // edit this, save, and watch it take effect live
    if (Input!.IsActionPressed("Jump"))
        Velocity.Y = JumpForce;
}
```

## What You Get for Free

- Direct `.NET 8` runtime access with modern language features
- Multithreading primitives without marshaling layers
- The same input state struct the editor itself uses

That is the entire onboarding. No codegen, no magic strings, no editor-side duplication.
