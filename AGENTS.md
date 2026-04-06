# AGENTS.md

## Project Summary
Path of Terraria is a large `tModLoader` mod that replaces a lot of vanilla Terraria progression with ARPG systems inspired by Diablo, Path of Exile, and Last Epoch.

Major pillars in this repo:

- Gear/itemization with affixes, rarity, implicits, sockets, maps, and unique items
- Active skills, skill trees, augments, passives, and class-adjacent progression
- A separate passive tree system driven by JSON data
- Quest-driven progression centered around Ravencrest and boss domains
- Mapping/subworld content, including reusable `MappingWorld` and boss domain subworlds
- Extensive custom UI and interaction layers

This is a large codebase. Do not start by reading random files under `Assets/`. Start from the subsystem relevant to the task.

## High-Level Layout

- `PoTMod.cs`
  - Mod entry point.
  - Important: `CreateDefaultContentSource()` redirects `Content/...` asset requests to `Assets/...`.
- `Common/`
  - Shared gameplay systems, registries, mechanics, mod players, questing, mapping, subworlds, worldgen, utilities.
  - This is where most reusable logic lives.
- `Content/`
  - Concrete game content: items, NPCs, skills, passives, projectiles, tiles, subworld-specific content.
  - If `Common` is framework/base logic, `Content` is the actual mod content built on top of it.
- `Core/`
  - Infrastructure: UI manager, item hook plumbing, graphics, commands, low-level helpers, camera/audio/time/pathfinding.
- `Assets/`
  - Textures, sounds, effects, structures, load screens, UI art.
- `Localization/`
  - HJSON localization split by topic and language.
- `Common/Data/`
  - JSON-backed registries and data definitions.

## Where To Look By Task

### Itemization, affixes, rarity, gear, sockets

Start here:

- `Core/Items/PoTItemHelper.cs`
  - Central helper for item level selection, affix rolling, rerolling, and affix application.
- `Core/Items/PoTGlobalItem.cs`
  - Broad Path of Terraria item behavior for all supported items.
- `Core/Items/GearGlobalItem.cs`
  - Socket behavior, gear prefix/suffix pools, equip/unequip handling, tooltip insertion.
- `Core/Items/ItemDatabase.cs`
  - Registers droppable items and maps vanilla items into PoT itemization.
- `Content/Items/Gear/Gear.cs`
  - Base class for modded gear items.
- `Common/Systems/Affixes/`
  - Affix base classes and player-side affix tracking.
- `Common/Data/AffixRegistry.cs`
  - Loads affix distributions from JSON and resolves tier/value data.

Important patterns:

- Item behavior is split between base item classes and global item systems.
- Affix data is partly data-driven:
  - code type in `Common/Systems/Affixes/ItemTypes/*.cs`
  - roll/tier metadata in `Common/Data/Affixes/*.json`
- Vanilla items can be opted into the gear system through `ItemDatabase`.
- Sockets are handled through `GearGlobalItem` and `Content/Socketables`.

### Skills, skill trees, augments, skill passives, skill specials

Start here:

- `Common/Mechanics/Skill.cs`
  - Base active skill abstraction.
- `Common/Systems/Skills/SkillTree.cs`
  - Singleton-style definition of a skill tree.
- `Common/Systems/Skills/SkillTreePlayer.cs`
  - Multiplayer sync and per-player cached skill tree state.
- `Common/Systems/ModPlayers/SkillPlayers/SkillCombatPlayer.cs`
  - Hotbar skill slots, keybinds, using/equipping skills.
- `Common/Mechanics/SkillPassive.cs`
- `Common/Mechanics/SkillSpecial.cs`
- `Common/Mechanics/SkillAugment.cs`

Concrete content lives in:

- `Content/Skills/`
- `Content/SkillPassives/`
- `Content/SkillSpecials/`
- `Content/SkillAugments/`
- `Content/SkillTrees/`

Important gotchas:

- `SkillTree` instances are effectively shared definitions. Player-specific load state is delayed and applied through `SkillTreePlayer`.
- Do not treat `SkillTree` fields as safe per-player mutable storage outside the established load/sync flow.
- If a skill changes visuals or localization based on specialization, inspect `Skill.Texture`, `Skill.DisplayName`, and `Skill.Description`.

### Passive tree

Start here:

- `Common/Systems/PassiveTreeSystem/PassiveTreePlayer.cs`
- `Common/Systems/PassiveTreeSystem/Passive.cs`
- `Common/Data/PassiveRegistry.cs`
- `Common/Data/Passives/Passives.json` or `Passives-dev.json`

Important patterns:

- The passive tree is heavily data-driven from JSON.
- Node definitions, positions, and connections come from the registry JSON.
- Code for actual effects lives in `Content/Passives/`.
- `PassiveTreePlayer.StrengthByPassive` is the fast path for "what does this player currently have allocated?"

### Quests and progression

Start here:

- `Common/Systems/Questing/Quest.cs`
- `Common/Systems/Questing/QuestModPlayer.cs`
- `Common/Systems/Questing/QuestSystem.cs`
- `Common/Systems/Questing/QuestStepTypes/`
- `Common/Systems/Questing/RewardTypes/`
- `Common/Systems/Questing/Quests/MainPath/`

Concrete quest-giver NPCs are usually in:

- `Content/NPCs/Town/`

Common pattern:

1. A town NPC decides which quest is available.
2. That NPC starts the quest through `QuestModPlayer`.
3. The quest itself defines steps/rewards in `Common/Systems/Questing/Quests/...`
4. Quest progression may also upgrade Ravencrest state or conditional drops.

If a task mentions "story progression," "boss unlocks," or "why isn't this NPC offering X," start with quest files and the corresponding town NPC.

### Mapping, boss domains, subworlds

Start here:

- `Common/Subworlds/MappingWorld.cs`
  - Base class for map/boss-domain subworlds.
- `Common/Subworlds/BossDomainSubworld.cs`
  - Base class for boss domains.
- `Common/Subworlds/RavencrestSubworld.cs`
  - Hub town subworld.
- `Common/Mapping/MapDeviceInterface.cs`
  - The large custom UI for the map device.
- `Content/Items/Consumables/Maps/Map.cs`
  - Base item for maps.

Concrete domains/areas live in:

- `Common/Subworlds/BossDomains/`
- `Common/Subworlds/MappingAreas/`

Important gotchas:

- `MappingWorld.AreaLevel`, `MapTier`, and `Affixes` are static state used across domain entry.
- World/subworld state transfer relies on `CopyMainWorldData`, `ReadCopiedMainWorldData`, `CopySubworldData`, and `ReadCopiedSubworldData`.
- If you change progression-sensitive domain behavior, inspect tracker sync in `MappingWorld` and `Common/Systems/Synchronization/Handlers/`.

### World generation and Ravencrest

Start here:

- `Common/World/GenerationSystem.cs`
- `Common/World/AutoGenStep.cs`
- `Common/World/Passes/`
- `Common/World/Generation/`
- `Common/Subworlds/RavencrestContent/`

Important patterns:

- Overworld generation is inserted through `AutoGenStep` implementations.
- Ravencrest is a real subworld with copied state and structure placement, not just a UI scene.
- Structures are often placed from assets under `Assets/Structures/`.

### UI

Start here:

- `Core/UI/UIManager.cs`
  - Base UI registration/render insertion.
- `Core/UI/SmartUI/`
  - Higher-level UI framework used by many mod UIs.
- `Common/UI/`
  - Most gameplay UI states.
- `Common/Mapping/MapDeviceInterface.cs`
  - One of the largest and most custom UI files in the repo.

Important pattern:

- This mod has its own UI registration layer on top of tModLoader.
- Before changing a UI, search for both the specific UI state and `SmartUiLoader`.

### NPCs and town content

Start here:

- `Content/NPCs/Town/`
- `Common/NPCs/`
- `Common/NPCs/Dialogue/`
- `Common/NPCs/QuestMarkers/`

Town NPCs often combine:

- quest offering
- custom dialogue components
- Ravencrest spawn positioning
- shop gating based on quest progression

### Multiplayer / sync

Start here:

- `Common/Systems/Synchronization/Networking.cs`
- `Common/Systems/Synchronization/Handlers/`

If you add state that matters outside singleplayer, search first for similar handlers rather than inventing ad hoc packet code.

## Base Types And Extension Points

When adding or changing content, find the nearest base type first.

- Gear items: `Content/Items/Gear/Gear.cs`
- Maps: `Content/Items/Consumables/Maps/Map.cs`
- Skills: `Common/Mechanics/Skill.cs`
- Skill tree nodes:
  - `Common/Mechanics/SkillPassive.cs`
  - `Common/Mechanics/SkillSpecial.cs`
  - `Common/Mechanics/SkillAugment.cs`
- Passive tree passives: `Common/Systems/PassiveTreeSystem/Passive.cs`
- Quests: `Common/Systems/Questing/Quest.cs`
- Quest steps: `Common/Systems/Questing/QuestStep.cs` and `QuestStepTypes/*`
- Subworlds: `Common/Subworlds/MappingWorld.cs` or `BossDomainSubworld.cs`
- Waypoints: `Common/Waypoints/ModWaypoint.cs`

In general, avoid implementing a feature directly in a concrete content file until you have checked whether the subsystem already has a hook, interface, or base class for it.

## Data-Driven Areas

The repo is not purely code-driven.

Important data sources:

- `Common/Data/Affixes/*.json`
  - Affix tier/value metadata.
- `Common/Data/Passives/*.json`
  - Passive tree structure data.
- `Common/Data/VanillaItemData/*.json`
  - Vanilla item classification into PoT itemization.
- `Localization/*/*.hjson`
  - Split localization by topic.

Before changing code behavior, verify whether the actual behavior is defined by JSON or localization data.

## Common Search Strategy

For most tasks, this order is efficient:

1. Find the concrete content class.
2. Find its base class in `Common` or `Content`.
3. Find the relevant `ModPlayer`, `ModSystem`, or global item/NPC/projectile hook.
4. Check sync handlers if the feature is player-visible in multiplayer.
5. Check localization and assets last.

Useful searches:

- `rg "TryAddSkill|UseSkill|SkillTreePlayer|SkillCombatPlayer" Common Content`
- `rg "QuestModPlayer|StartQuest|CanStartQuest" Common Content`
- `rg "MappingWorld|BossDomainSubworld|OpenMap|MapDevice" Common Content`
- `rg "GenerateAffixes|GenerateImplicits|PoTItemHelper|GearGlobalItem" Core Common Content`
- `rg "PassiveTreePlayer|PassiveRegistry|ReferenceId" Common Content`

## Project-Specific Gotchas

- `Content` in asset paths is redirected to `Assets` by `PoTMod.CreateDefaultContentSource()`.
- Many gameplay systems rely on `ModPlayer` state. If a change appears to "not stick," inspect save/load and `ResetEffects`.
- Skill trees are shared definitions with delayed per-player load. Be careful with stateful fields.
- Mapping/subworld systems copy progression state manually. Changing world/domain flow often requires sync/copy updates.
- A lot of progression is quest-gated rather than purely boss-flag-gated.
- Vanilla behavior is intentionally overridden in multiple places under `Common/Systems/VanillaModifications/`.

## Build And Verification

Primary project file:

- `PathOfTerraria.csproj`

Build notes:

- The project imports `..\tModLoader.targets`.
- References include `SubworldLibrary`, `HousingAPI`, `NPCUtils`, `StructureHelper`, and `Wayfarer` from `lib/`.
- Custom shader build tools live under `BuildTools/ShaderCompiler/`.

If you are validating a change, prefer:

1. `dotnet build PathOfTerraria.csproj`
2. targeted code search for similar patterns
3. if relevant, check save/load methods and synchronization handlers

## Practical Guidance For Future Agents

- Start narrow. This repo is too large for blind reading.
- Use `Common` for framework logic and `Content` for feature implementations.
- If the task is about progression, always check quests and Ravencrest interactions.
- If the task is about loot, always check both the item class and the global item/helper path.
- If the task is about skills, check both the active skill and its tree/passives/specializations.
- If the task is about maps/domains, check item, UI, subworld, and sync together.
- If the task is about a UI bug, check both `Common/UI/...` and `Core/UI/...`.
