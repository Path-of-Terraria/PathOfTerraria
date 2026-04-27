# Gear System Design

## Core Statement

Path of Terraria replaces Terraria's static, class-locked gear progression with an ARPG itemization system inspired by Diablo, Path of Exile, and Last Epoch. Every piece of gear is procedurally generated with an item level, a rarity tier, and a pool of randomized affixes. Gear grows in power alongside world progression — not through crafting specific ore tiers, but through drops that scale to whichever bosses have been defeated and which maps the player is running. The system is designed so that a well-rolled PoT item is always worth seeking out, and vanilla armor is intentionally **not** a viable long-term substitute.

---

## Design Goals

1. **Every slot matters.** Helmet, chestplate, leggings, rings, amulet, and offhand should each represent a meaningful upgrade decision at every stage of the game.

2. **Progression through drops, not crafting gates.** Item level tracks world progression (boss kills, map tier) so that better gear becomes available naturally as the player advances, without gating upgrades behind specific ore or crafting recipes.

3. **Player agency over character power.** Rarity, affixes, sockets, and rerolling give players meaningful choices about which items to keep and invest in, rather than simply following a fixed upgrade path.

4. **Vanilla armor must not compete.** PoT armor is purpose-built to scale with the itemization system. Vanilla armor offers static defense and set bonuses that remain competitive indefinitely. For PoT armor to have real value, vanilla armor must be clearly inferior past early-game.

5. **Unique items as memorable milestones.** Uniques should be recognizable by name, visually distinct, and carry fixed affixes that define a specific playstyle rather than being a simple upgrade.

6. **Transparency.** Tooltips show item level, rarity, affix tiers, roll ranges, and implicit vs. explicit distinction so players can evaluate gear without guessing.

---

## Design Pillars

### Item Level Scales With World Progression

Item level is the central axis of gear power. It is determined by defeated bosses in the overworld and by `AreaLevel` inside maps:

| Milestone | Item Level |
|---|---|
| World start | 5 |
| King Slime | 10 |
| Eye of Cthulhu | 15 |
| Eater of Worlds / Brain | 20 |
| Both corruption bosses | 25 |
| Queen Bee | 25–30 |
| Deerclops | 30–35 |
| Skeletron | 35–40 |
| Wall of Flesh (hardmode floor) | 45 |
| Queen Slime | 50 |
| Twins | 55 |
| Destroyer | 60 |
| Skeletron Prime | 65 |
| Plantera | 70 |
| Golem | 75 |
| Cultist | 80 |
| Moon Lord | 85 |

Crafted items are capped at item level 45 (the hardmode minimum). Dropped items and map items use the full unclamped scale.

### Rarity Determines Affix Count

| Rarity | Affix Count | Max Affixes |
|---|---|---|
| Normal | 0 | 0 |
| Magic | 1–2 | 2 |
| Rare | 3–4 | 4 |
| Unique | Fixed pool | Fixed pool |

Implicits are guaranteed, non-rerollable affixes set by the item type itself. Explicit affixes are randomized and can be rerolled, removed, or augmented.

### Defense Scales With Item Level (PoT Armor Only)

PoT armor's base defense is computed from item level at roll time:

- **Helmet:** `itemLevel / 10 + 1`
- **Chestplate:** `itemLevel / 6 + 1`
- **Leggings:** `itemLevel / 12 + 1`

This means a max-level helmet (item level 85) provides 9 base defense before any affixes, while a max-level chestplate reaches 15. Affixes on top of that (e.g., `DefenseItemAffix`, `EnduranceItemAffix`) push totals significantly higher and make PoT armor definitively stronger than anything vanilla provides at the equivalent stage.

### Sockets Provide Lateral Power

Gear can contain sockets filled with Socketables. These provide persistent bonuses while equipped and are removed without destruction when unequipping. Sockets add build diversity without invalidating item base stats.

### Maps Are Gear Delivery Mechanisms

Maps have their own item level (`AreaLevel`) that overrides overworld progression when active. Higher-tier maps guarantee higher item level drops, creating a farming loop where players can target specific power bands by choosing appropriate map tiers.

---

## Gear System Architecture

### Item Identity

Each item carries two data records:

- **`PoTStaticItemData`** — shared across all instances of the same item type. Stores `DropChance`, `IsUnique`, and `ItemType`.
- **`PoTInstanceItemData`** — per-instance. Stores `ItemType`, `Rarity`, item level (`RealLevel`), `ImplicitCount`, and the `Affixes` list.

### Rolling Pipeline

When an item is created or dropped it goes through `PoTItemHelper.Roll(item, itemLevel)`:

1. Item level is assigned via `SetItemLevel`.
2. Implicit affixes are generated via `GenerateImplicits.IItem` and locked in `ImplicitCount`.
3. Explicit affixes are rolled via `GenerateAffixes.IItem` based on rarity.
4. `PostRoll.IItem` fires — this is where armor sets its base `Item.defense` from item level.
5. A display name suffix is generated via `GenerateNameAffixes`.

### Affix Registry

Affix metadata (tiers, value ranges, eligible item types, minimum item level per tier) lives in JSON files under `Common/Data/Affixes/`. The code types for each affix live in `Common/Systems/Affixes/ItemTypes/`. This split means adding a new affix tier or rebalancing values does not require a code change.

Example structure (`ArmorAffixes.json`):
```json
{
  "affixType": "DefenseItemAffix",
  "equipTypes": "Armor Shield",
  "tiers": [
    { "minValue": 1,  "maxValue": 4,  "minimumLevel": 1,  "weight": 1 },
    { "minValue": 5,  "maxValue": 8,  "minimumLevel": 21, "weight": 1 },
    { "minValue": 9,  "maxValue": 12, "minimumLevel": 52, "weight": 1 },
    { "minValue": 13, "maxValue": 16, "minimumLevel": 73, "weight": 1 }
  ]
}
```

### Gear Class Hierarchy

```
ModItem
└── Gear                    (Content/Items/Gear/Gear.cs)
    ├── Helmet              (Content/Items/Gear/Armor/Helmet/Helmet.cs)
    │   ├── Visor
    │   ├── Crown
    │   ├── CrystalVisor
    │   └── FirelordsWill   (Unique)
    ├── Chestplate          (Content/Items/Gear/Armor/Chestplate/Chestplate.cs)
    │   ├── BodyArmor
    │   ├── Breastplate
    │   └── PyralisHeart    (Unique)
    ├── Leggings            (Content/Items/Gear/Armor/Leggings/Leggings.cs)
    ├── Ring                (Content/Items/Gear/Rings/Ring.cs)
    ├── Amulet              (Content/Items/Gear/Amulets/Amulet.cs)
    ├── Offhand             (Content/Items/Gear/Offhands/Offhand.cs)
    │   ├── Shield
    │   ├── Focus
    │   ├── Quiver
    │   └── Talisman
    └── Weapon types        (swords, bows, staves, wands, whips, etc.)
```

### Global Item Hooks

- **`PoTGlobalItem`** — applies to any item with damage, defense, accessory flag, or armor slot. Fires `Roll` on `SetDefaults` and applies affixes in `UpdateEquip`.
- **`GearGlobalItem`** — applies to `Gear` subclasses and opted-in vanilla items. Manages socket behavior, equip/unequip events, and prefix/suffix pools by `GearLocalizationCategory`.
- **`AppliesToEntity`** filters out coins and vanity items so the system only touches meaningful gear.

### Vanilla Item Integration

Vanilla items can be opted into the PoT itemization system through `ItemDatabase` and their corresponding JSON files in `Common/Data/VanillaItemData/`. These files assign an `ItemType` (e.g., `"Helmet"`) and element proportions, making vanilla items eligible for affix rolling alongside PoT items when explicitly enrolled.

---

## Known Problems & Resolutions

### Problem: Vanilla Armor Is Always Viable

**Root cause.** Vanilla armor has fixed defense values and set bonuses baked into Terraria's code. These values do not erode relative to PoT gear as the player progresses, and vanilla set bonuses provide class-specific benefits that PoT armor currently cannot replicate slot-for-slot.

**Why this matters.** If vanilla armor remains competitive — especially in hardmode — players have no incentive to engage with the PoT gear system in armor slots. This undermines the investment made in PoT armor content and affix design.

**Resolution.** Vanilla armor should be explicitly nerfed via a `ModPlayer.PostUpdateEquips` (or equivalent `GlobalItem.UpdateEquip`) hook that detects equipped vanilla (non-PoT) armor and applies a scaling defense reduction as world progression advances. The penalty should be:
- Minimal in pre-hardmode, so vanilla armor still serves as a functional starter option.
- Significant (50–70% effective defense reduction) on hardmode entry, making any PoT drop clearly superior.

PoT armor pieces are immune because they re-derive defense at roll time from item level. Vanilla items enrolled in `ItemDatabase` can be granted an exemption flag if they are intended to participate in the full PoT system.

A corresponding tooltip line should communicate to players why their vanilla armor feels weaker, framing it as the gear system encouraging them to upgrade.

---

### Problem: Item Level Crafting Cap Conflicts With Hardmode

**Root cause.** Crafted gear is capped at item level 45 (the hardmode entry floor) to prevent players from trivially crafting max-power items. However, this means crafted PoT gear is perpetually outdone by dropped gear from the same stage.

**Resolution.** Crafting is intended as a fallback, not the primary upgrade path. The cap is intentional. Players should be directed toward drops and maps for meaningful upgrades. Crafting recipes can still produce items with up to 4 affixes at item level 45 — competitive for early hardmode but outpaced by item level 55+ drops after the first hardmode bosses.

---

### Problem: Vanilla Items Opted Into PoT Still Compete Unfairly

**Root cause.** Some vanilla items (e.g., adamantite armor) are enrolled in `VanillaItemData` with an `ItemType` but retain their vanilla defense values and set bonuses on top of any PoT affixes they receive.

**Resolution.** When a vanilla item is enrolled in the PoT system, its base vanilla defense contribution should be overridden or zeroed so that the PoT affix pool becomes the sole source of defensive stats, consistent with how PoT armor base types work.
