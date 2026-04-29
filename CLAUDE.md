# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Read AGENTS.md first

`AGENTS.md` at the repo root is the authoritative architecture map for this codebase. It documents:

- The Common / Content / Core / Assets split and what belongs where
- Where to start by task (itemization, skills, passive tree, quests, mapping/subworlds, worldgen, UI, NPCs, multiplayer sync)
- Base types and extension points for adding new content
- Data-driven areas (`Common/Data/**/*.json`, `Localization/**/*.hjson`)
- Project-specific gotchas (the `Content`→`Assets` asset redirect, `ModPlayer.ResetEffects` ordering, shared `SkillTree` definitions, manual subworld state copying, quest-gated progression)

Do not duplicate that file's guidance here. When a task touches gameplay systems, work from `AGENTS.md`'s "Where To Look By Task" section before searching blindly — this is a large mod and random reading is wasteful.

## Build, lint, run

This is a tModLoader mod targeting `net8.0`. It builds against tModLoader via `..\tModLoader.targets` (one directory above the repo root) — see `.github/README.md` for the local setup, including how to wire up `SubworldLibrary` from the Steam Workshop install.

- Build: `dotnet build PathOfTerraria.sln --configuration Release`
- Lint (CI runs this and fails on diff): `dotnet format style PathOfTerraria.sln --verify-no-changes --severity error`
- Run: launch from the IDE — the `BuildMod` target in `PathOfTerraria.csproj` invokes tML's `-build` against `$(tMLServerPath)` after a normal `dotnet build`, then tML loads the mod.

CI (`.github/workflows/ci.yml`) runs build + lint on `main` and `develop`. The active development branch is `develop`; PRs typically target it.

## Banned APIs

`BannedSymbols.txt` is enforced via `Microsoft.CodeAnalysis.BannedApiAnalyzers`. Currently banned:

- `System.Reflection.Assembly.GetTypes()` — use `AssemblyManager.GetLoadableTypes(Assembly)` instead. `GetTypes()` can throw in tModLoader's weakref scenarios.

If a build fails with `RS0030`, this is why.

## Global usings

`Usings.cs` globally imports `Microsoft.Xna.Framework[.Graphics]`, `System`, `System.Diagnostics`, `Terraria`, `Terraria.ModLoader`, and `PathOfTerraria.Core.Debugging`. Don't re-add these `using`s in individual files.

## Asset path redirect

`PoTMod.CreateDefaultContentSource()` rewrites any `Content/...` asset path to `Assets/...` via `SmartContentSource.AddDirectoryRedirect`. Code references like `Mod/Content/Items/Foo` resolve to files under `Assets/Items/Foo`. This is the most common source of "asset not found" confusion — textures live in `Assets/`, not next to their `.cs` files.

## Versioning

`build.txt` holds the mod `version`. `scripts/increment-version.py` bumps it; prefer the script over editing by hand.
