---
layout: post
title: Welcome to the Kolpa Engine Blog
description: Why this blog exists, what we are building, and how to follow along.
date: 2026-08-01
author: Gabriel Aplok
tags:
  - announcement
  - engine
---

This is the first post on the Kolpa Engine blog. If you are reading this, you are early.

## What Kolpa Is

Kolpa is a modular, hackable open-source game engine built with Vulkan and Direct3D 12. It is designed for developers who want performance, direct memory control, and custom workflows without engine bloat.

The engine is written in C++ at the core, with a C# (.NET 8) scripting layer on top. You get type-safe gameplay code, multithreading, and direct access to the renderer when you need it.

## Why a Blog

We want to document decisions as they happen. Engine development produces a lot of context that gets lost in commits and issues. Posts here will cover renderer work, tooling, and the occasional detour into whatever problem we were debugging that week.

## What Is Coming

- Deep dives into the clustered forward renderer
- How script hot reload actually works
- Notes on the custom UI framework
- Benchmarks, build pipeline details, and release notes

If something sounds interesting, you can follow the repo at [github.com/kolpa-engine](https://github.com/kolpa-engine).
