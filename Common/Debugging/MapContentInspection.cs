#if DEBUG
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PathOfTerraria.Common.Conflux;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Subworlds.MappingAreas;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.Maps;
using PathOfTerraria.Common.Systems.MapContent;
using PathOfTerraria.Common.Systems.MobSystem;
using PathOfTerraria.Common.Systems.Runebound;
using PathOfTerraria.Common.Systems.Sigils;
using PathOfTerraria.Content.Conflux;
using PathOfTerraria.Content.Tiles.Maps;
using SubworldLibraryCommunityFork;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Debugging;

internal readonly record struct MapInspectionEntry(string Tab, string Label, string Value, string Detail)
{
	internal void Write(BinaryWriter writer)
	{
		writer.Write(Tab);
		writer.Write(Label);
		writer.Write(Value);
		writer.Write(Detail);
	}

	internal static MapInspectionEntry Read(BinaryReader reader)
	{
		return new(reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadString());
	}
}

internal sealed class MapInspectionSnapshot
{
	internal ulong CapturedTick { get; private set; }
	internal string Source { get; private set; } = string.Empty;
	internal List<MapInspectionEntry> Entries { get; } = [];

	internal void Add(string tab, string label, object value, string detail = "")
	{
		Entries.Add(new(tab, label, value?.ToString() ?? "-", detail));
	}

	internal IEnumerable<MapInspectionEntry> ForTab(string tab)
	{
		return Entries.Where(entry => entry.Tab == tab);
	}

	internal void Write(BinaryWriter writer)
	{
		writer.Write(CapturedTick);
		writer.Write(Source);
		writer.Write(Entries.Count);

		foreach (MapInspectionEntry entry in Entries)
		{
			entry.Write(writer);
		}
	}

	internal static MapInspectionSnapshot Read(BinaryReader reader)
	{
		var snapshot = new MapInspectionSnapshot
		{
			CapturedTick = reader.ReadUInt64(),
			Source = reader.ReadString(),
		};

		int count = reader.ReadInt32();
		for (int i = 0; i < count; i++)
		{
			snapshot.Entries.Add(MapInspectionEntry.Read(reader));
		}

		return snapshot;
	}

	internal static MapInspectionSnapshot Create(string source)
	{
		return new MapInspectionSnapshot { CapturedTick = Main.GameUpdateCount, Source = source };
	}
}

/// <summary>
/// Produces an authoritative snapshot of everything currently loaded in an exploration map.
/// Future map mechanics can register a provider to append their own exact telemetry.
/// </summary>
internal static class MapContentInspection
{
	internal const string OverviewTab = "Overview";
	internal const string ContentTab = "Content";
	internal const string ContainersTab = "Containers";
	internal const string ModifiersTab = "Modifiers";
	internal const string SigilsTab = "Sigils";
	internal const string DiagnosticsTab = "Diagnostics";

	private static readonly Dictionary<string, Action<MapInspectionSnapshot>> Providers = [];

	internal static MapInspectionSnapshot LatestSnapshot { get; set; }

	internal static bool IsInExplorationMap =>
		SubworldSystem.Current is MappingWorld and IExplorationWorld and not RavencrestSubworld;

	internal static void RegisterProvider(string key, Action<MapInspectionSnapshot> provider)
	{
		Providers[key] = provider;
	}

	internal static void UnregisterProvider(string key)
	{
		Providers.Remove(key);
	}

	internal static MapInspectionSnapshot Capture(int requestingPlayer, string source)
	{
		MapInspectionSnapshot snapshot = MapInspectionSnapshot.Create(source);
		if (!IsInExplorationMap || SubworldSystem.Current is not MappingWorld world)
		{
			snapshot.Add(OverviewTab, "Status", "Unavailable", "The inspector only captures exploration maps.");
			return snapshot;
		}

		Player player = requestingPlayer >= 0 && requestingPlayer < Main.maxPlayers && Main.player[requestingPlayer].active
			? Main.player[requestingPlayer]
			: Main.player.FirstOrDefault(candidate => candidate.active) ?? Main.player[0];

		AddOverview(snapshot, world);
		AddContent(snapshot);
		AddContainers(snapshot);
		AddModifiers(snapshot, world, player);
		AddSigils(snapshot);
		AddDiagnostics(snapshot, world);

		foreach ((string key, Action<MapInspectionSnapshot> provider) in Providers)
		{
			try
			{
				provider(snapshot);
			}
			catch (Exception exception)
			{
				snapshot.Add(DiagnosticsTab, $"Provider: {key}", "Failed", exception.Message);
			}
		}

		return snapshot;
	}

	private static void AddSigils(MapInspectionSnapshot snapshot)
	{
		snapshot.Add(SigilsTab, "Unlocked Device Slots", $"{SigilSystem.UnlockedSlotCount}/4",
			$"Highest completed exploration-map tier: {SigilSystem.HighestCompletedMapTier}.");
		snapshot.Add(SigilsTab, "Active Sigils", SigilSystem.ActiveSigils.Count);
		snapshot.Add(SigilsTab, "Map Threat", SigilSystem.ActiveSigils.Sum(SigilCatalog.GetThreat));
		for (int i = 0; i < SigilSystem.ActiveSigils.Count; i++)
		{
			SigilEntry entry = SigilSystem.ActiveSigils[i];
			snapshot.Add(SigilsTab, $"Slot {i + 1}", entry.Kind,
				$"Grade {entry.Grade}; family {entry.Family}; {SigilCatalog.DescribeEffect(entry)}.");
		}

		snapshot.Add(SigilsTab, "Nemesis Captains", $"{SigilEncounterSystem.SpawnedNemeses}/{SigilEncounterSystem.RequestedNemeses}");
		snapshot.Add(SigilsTab, "Warded Caches", $"{SigilEncounterSystem.SpawnedCaches}/{SigilEncounterSystem.RequestedCaches}");
		snapshot.Add(SigilsTab, "Consecrated Shrines", $"{SigilEncounterSystem.SpawnedShrines}/{SigilEncounterSystem.RequestedShrines}");
		int livingNemeses = Main.npc.Count(npc => npc.active && npc.GetGlobalNPC<SigilEncounterNPC>().Nemesis);
		int livingCaches = Main.npc.Count(npc => npc.active && npc.ModNPC is Content.NPCs.Mapping.Sigils.WardedCacheNPC);
		int livingShrines = Main.npc.Count(npc => npc.active && npc.ModNPC is Content.NPCs.Mapping.Sigils.ConsecratedShrineNPC);
		snapshot.Add(SigilsTab, "Living Sigil Content", livingNemeses + livingCaches + livingShrines,
			$"Captains {livingNemeses}; caches {livingCaches}; shrines {livingShrines}.");
	}

	private static void AddOverview(MapInspectionSnapshot snapshot, MappingWorld world)
	{
		snapshot.Add(OverviewTab, "Map", world.SubworldName?.Value ?? world.GetType().Name, world.FullName);
		snapshot.Add(OverviewTab, "Area Level", MappingWorld.AreaLevel);
		snapshot.Add(OverviewTab, "Map Tier", MappingWorld.MapTier);
		snapshot.Add(OverviewTab, "Map Affixes", MappingWorld.Affixes?.Count ?? 0);
		snapshot.Add(OverviewTab, "World Size", $"{Main.maxTilesX:N0} x {Main.maxTilesY:N0} tiles");
		snapshot.Add(OverviewTab, "Active Players", Main.player.Count(player => player.active));
		snapshot.Add(OverviewTab, "Active NPCs", Main.npc.Count(npc => npc.active));
		snapshot.Add(OverviewTab, "Active Projectiles", Main.projectile.Count(projectile => projectile.active));
		snapshot.Add(OverviewTab, "Chests", Main.chest.Count(chest => chest is not null));
		snapshot.Add(OverviewTab, "Tile Entities", TileEntity.ByID.Count);
	}

	private static void AddContent(MapInspectionSnapshot snapshot)
	{
		RuneboundSystem.DebugMapInfo runebound = RuneboundSystem.GetDebugMapInfo();
		snapshot.Add(ContentTab, "Runebound Encounters", $"{runebound.CompletedCount + runebound.ActiveCount}/{runebound.TargetCount}",
			$"Completed {runebound.CompletedCount}; loaded {runebound.ActiveCount}; remaining to spawn {runebound.RemainingCount}.");

		foreach (RuneboundSystem.DebugEncounterInfo encounter in runebound.Encounters)
		{
			string state = encounter.Active ? "Released" : "Imprisoned";
			string position = encounter.Position == Vector2.Zero ? "unknown" : $"{encounter.Position.X / 16f:0}, {encounter.Position.Y / 16f:0} tiles";
			snapshot.Add(ContentTab, $"Runebound #{encounter.Id}", state,
				$"{encounter.Grade} {encounter.Family}; enemies {encounter.EnemyCount}; participants {encounter.ParticipantCount}; position {position}.");
		}

		ConfluxRift[] rifts = Main.projectile
			.Where(projectile => projectile.active)
			.Select(projectile => projectile.ModProjectile)
			.OfType<ConfluxRift>()
			.ToArray();
		snapshot.Add(ContentTab, "Conflux Rifts", rifts.Length,
			ConfluxRifts.DebugGenerationEvaluated
				? $"Legacy generation roll target: {ConfluxRifts.DebugGenerationTarget}."
				: "Legacy generation roll has not been evaluated yet.");

		foreach (IGrouping<ConfluxRiftKind, ConfluxRift> group in rifts.GroupBy(rift => rift.Kind))
		{
			int active = group.Count(rift => rift.Activated && !rift.Closing);
			snapshot.Add(ContentTab, $"Rift: {group.Key}", group.Count(), $"Active battles {active}; closing {group.Count(rift => rift.Closing)}.");
		}

		int shrineCount = TileEntity.ByID.Values.Count(entity => entity is BaseShrine.ShrineTileEntity);
		snapshot.Add(ContentTab, "Shrines", shrineCount);

		int strongboxCount = Main.npc.Count(npc => npc.active && npc.ModNPC?.GetType().Name.Contains("Strongbox", StringComparison.OrdinalIgnoreCase) == true)
			+ Main.projectile.Count(projectile => projectile.active && projectile.ModProjectile?.GetType().Name.Contains("Strongbox", StringComparison.OrdinalIgnoreCase) == true)
			+ TileEntity.ByID.Values.Count(entity => entity.GetType().Name.Contains("Strongbox", StringComparison.OrdinalIgnoreCase));
		snapshot.Add(ContentTab, "Strongboxes", strongboxCount,
			strongboxCount == 0 ? "No strongbox content implementation is registered in this build." : "Detected by content type name.");

		var bosses = Main.npc.Where(npc => npc.active && npc.boss).GroupBy(npc => npc.ModNPC?.FullName ?? npc.TypeName);
		foreach (IGrouping<string, NPC> boss in bosses)
		{
			snapshot.Add(ContentTab, $"Boss: {boss.Key}", boss.Count());
		}

		var mapProjectiles = Main.projectile
			.Where(projectile => projectile.active && projectile.ModProjectile is IMapIcon)
			.GroupBy(projectile => projectile.ModProjectile.GetType().Name);
		foreach (IGrouping<string, Projectile> group in mapProjectiles)
		{
			snapshot.Add(ContentTab, $"Map Entity: {group.Key}", group.Count());
		}
	}

	private static void AddContainers(MapInspectionSnapshot snapshot)
	{
		Chest[] chests = Main.chest.Where(chest => chest is not null).ToArray();
		int nonEmpty = chests.Count(chest => chest.item.Any(item => item is { IsAir: false }));
		int itemStacks = chests.Sum(chest => chest.item.Count(item => item is { IsAir: false }));
		snapshot.Add(ContainersTab, "All Chests", chests.Length, $"Non-empty {nonEmpty}; empty {chests.Length - nonEmpty}; item stacks {itemStacks}.");

		foreach (IGrouping<string, Chest> group in chests.GroupBy(GetChestTypeName).OrderByDescending(group => group.Count()))
		{
			int groupStacks = group.Sum(chest => chest.item.Count(item => item is { IsAir: false }));
			snapshot.Add(ContainersTab, group.Key, group.Count(), $"Contains {groupStacks} item stacks.");
		}

		foreach (IGrouping<string, TileEntity> group in TileEntity.ByID.Values.GroupBy(entity => entity.GetType().Name).OrderByDescending(group => group.Count()))
		{
			snapshot.Add(ContainersTab, $"Tile Entity: {group.Key}", group.Count());
		}
	}

	private static string GetChestTypeName(Chest chest)
	{
		if (!WorldGen.InWorld(chest.x, chest.y))
		{
			return "Invalid/out-of-world chest";
		}

		Tile tile = Main.tile[chest.x, chest.y];
		ModTile modTile = ModContent.GetModTile(tile.TileType);
		return modTile?.FullName ?? $"Vanilla tile {tile.TileType}";
	}

	private static void AddModifiers(MapInspectionSnapshot snapshot, MappingWorld world, Player player)
	{
		List<MapAffix> affixes = MappingWorld.Affixes ?? [];
		float totalStrength = affixes.Sum(affix => affix.Strength);
		snapshot.Add(ModifiersTab, "Total Map Strength", totalStrength.ToString("#0.##"));
		snapshot.Add(ModifiersTab, "Experience Boost", $"+{totalStrength / 2f:#0.###}%");
		snapshot.Add(ModifiersTab, "Drop Rate Increase", $"+{ArpgNPC.DomainDropRateBoost(totalStrength) * 100f:#0.###}%");
		snapshot.Add(ModifiersTab, "Drop Rarity Increase", $"+{ArpgNPC.DomainRarityBoost(totalStrength) * 100f:#0.##}%");

		foreach (MapAffix affix in affixes)
		{
			var tooltips = new AffixTooltips();
			affix.ApplyTooltips(player, ItemType.Map, MappingWorld.AreaLevel, tooltips);
			string effect = string.Join("; ", tooltips.Lines.Values.Select(tip =>
				tip.Text.Format(Math.Abs(tip.Value).ToString("#0.##"), tip.Value >= 0 ? "+" : "-")));
			snapshot.Add(ModifiersTab, affix.Name, $"Tier {affix.Tier + 1}",
				$"Strength {affix.Strength:#0.##}; raw value {affix.Value:#0.##}. {effect}");
		}

		(int forcedTime, bool forcedDay) = world.ForceTime;
		snapshot.Add(ModifiersTab, "Forced Time", forcedTime < 0 ? "No" : $"{forcedTime} ({(forcedDay ? "day" : "night")})");
		snapshot.Add(ModifiersTab, "Mining Whitelist", world.WhitelistedMiningTiles.Length, "Explicit map-specific tile types.");
		snapshot.Add(ModifiersTab, "Placement Whitelist", world.WhitelistedPlaceableTiles.Length, "Explicit map-specific tile types.");
		snapshot.Add(ModifiersTab, "Explosion Whitelist", world.WhitelistedExplodableTiles.Length, "Explicit map-specific tile types.");
	}

	private static void AddDiagnostics(MapInspectionSnapshot snapshot, MappingWorld world)
	{
		RuneboundSystem.DebugMapInfo runebound = RuneboundSystem.GetDebugMapInfo();
		snapshot.Add(DiagnosticsTab, "Snapshot Source", snapshot.Source);
		snapshot.Add(DiagnosticsTab, "Captured Tick", snapshot.CapturedTick);
		snapshot.Add(DiagnosticsTab, "Subworld Type", world.GetType().FullName ?? world.GetType().Name);
		snapshot.Add(DiagnosticsTab, "Subworld Save", MappingWorld.LastSubworldSavePath ?? "Not assigned");
		snapshot.Add(DiagnosticsTab, "Saved Subworld Type", MappingWorld.LastSubworldFullName ?? "Not assigned");
		snapshot.Add(DiagnosticsTab, "Runebound Spawn Retry", $"{runebound.SpawnAttemptTimer} ticks");
		snapshot.Add(DiagnosticsTab, "Conflux Roll Evaluated", ConfluxRifts.DebugGenerationEvaluated);
		snapshot.Add(DiagnosticsTab, "Conflux Generation Target", ConfluxRifts.DebugGenerationTarget);
		snapshot.Add(DiagnosticsTab, "Registered Providers", Providers.Count,
			Providers.Count == 0 ? "Built-in collectors only." : string.Join(", ", Providers.Keys));
	}

	internal static void Clear()
	{
		LatestSnapshot = null;
	}

	internal static void Unload()
	{
		LatestSnapshot = null;
		Providers.Clear();
	}
}

internal sealed class MapContentInspectionSystem : ModSystem
{
	public override void ClearWorld()
	{
		MapContentInspection.Clear();
	}

	public override void Unload()
	{
		MapContentInspection.Unload();
	}
}
#endif
