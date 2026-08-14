---
layout: post
title: Welcome to the Kolpa Engine Blog
description: Why this blog exists, what we are building, and how to follow along.
date: 2026-08-01
author: Gabriel Aplok
image: /assets/features/editor.png
image_alt: Kolpa Engine editor viewport
tags:
  - announcement
  - engine
---

This is the first post on the Kolpa Engine blog. If you are reading this, you are early.

## What Kolpa Is

Kolpa is an open source game engine written in C and C++. It has two layers:

1. **kolpa** (C99): a game framework forked from raylib. Window, input, rendering (OpenGL), audio, and math.
2. **kolpa engine** (C++20): an object-driven, ECS-based engine built on the kolpa library. Scene management, physics, audio DSP, resource pipelines, and optional C# scripting via .NET 10.

You can use the framework alone for simple games, or the full engine for complex projects.

## Why a Blog

We want to document decisions as they happen. Engine development produces a lot of context that gets lost in commits and issues. Posts here will cover the ECS architecture, physics integration, scripting system, and the occasional detour into whatever problem we were debugging that week.

## What Is Coming

- How the flecs-based ECS works with GameObjects and MonoBehaviours
- The box3d physics integration and character controller
- C# scripting with hot reload via .NET 10
- The resource pipeline and byte-baked assets
- Scene JSON save/load and the Godot level editor

If something sounds interesting, you can follow the repo at [github.com/gabriel-aplok/kolpa-lib](https://github.com/gabriel-aplok/kolpa-lib).
