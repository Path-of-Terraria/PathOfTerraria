using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds;

internal class RavencrestSubworld : MappingWorld
{
	public override int Width => 900;
	public override int Height => 340;
	public override bool ShouldSave => true;
	public override int[] WhitelistedMiningTiles => [TileID.Tombstones];
	public override int[] WhitelistedPlaceableTiles => [TileID.Tombstones];

	public override List<GenPass> Tasks => [new FlatWorldPass(126, true, null, TileID.Dirt, WallID.Dirt), 
		new PassLegacy("World", SpawnWorld), new PassLegacy("Smooth", SmoothPass)];

	private void SmoothPass(GenerationProgress progress, GameConfiguration configuration)
	{
		for (int i = 0; i < Main.maxTilesX; ++i)
		{
			for (int j = 0; j < Main.maxTilesY; ++j) 
			{
				WorldGen.TileFrame(i, j, true);
			}
		}
	}

	public override void CopyMainWorldData()
	{
		base.CopyMainWorldData();

		SubworldSystem.CopyWorldData("smashedOrb", WorldGen.shadowOrbSmashed); // Copies this bool over since TownScoutNPC needs this
		SubworldSystem.CopyWorldData("orbsSmashed", (short)DisableEvilOrbBossSpawning.ActualOrbsSmashed); // Copies this bool over since Morven/Whispers of the Deep quest needs this
		SubworldSystem.CopyWorldData("time", Main.time); // Keeps time consistent
		SubworldSystem.CopyWorldData("dayTime", Main.dayTime); // Keeps time consistent
		SubworldSystem.CopyWorldData("overworldNPCs", ModContent.GetInstance<RavencrestSystem>().HasOverworldNPC.ToArray());
		SubworldSystem.CopyWorldData("hardMode", Main.hardMode);
		ModContent.GetInstance<PersistentDataSystem>().CopyDataToRavencrest();
	}

	public override void ReadCopiedMainWorldData()
	{
		base.ReadCopiedMainWorldData();

		WorldGen.shadowOrbSmashed = SubworldSystem.ReadCopiedWorldData<bool>("smashedOrb");
		Main.time = SubworldSystem.ReadCopiedWorldData<double>("time");
		Main.dayTime = SubworldSystem.ReadCopiedWorldData<bool>("dayTime");
		// Mirrors the write in CopyMainWorldData. Without this read, the value saved in Ravencrest's .wld (forced false
		// during worldgen) wins on every entry, which strands hardmode-gated NPCs like Pyra (TinkerNPC) and any quest or
		// dialogue check that asks Main.hardMode while inside Ravencrest.
		Main.hardMode = SubworldSystem.ReadCopiedWorldData<bool>("hardMode");
		DisableEvilOrbBossSpawning.ActualOrbsSmashed = SubworldSystem.ReadCopiedWorldData<short>("orbsSmashed");

		ModContent.GetInstance<RavencrestSystem>().HasOverworldNPC.Clear();
		string[] set = SubworldSystem.ReadCopiedWorldData<string[]>("overworldNPCs");

		foreach (string name in set)
		{
			ModContent.GetInstance<RavencrestSystem>().HasOverworldNPC.Add(name);
		}

		ModContent.GetInstance<PersistentDataSystem>().ReadDataInRavencrest();
	}

	private void SpawnWorld(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.spawnTileX = 316;
		Main.spawnTileY = 156;

		ModContent.GetInstance<RavencrestSystem>().CurrentRavencrestVersion = RavencrestSystem.RavencrestVersion;
		StructureTools.PlaceByOrigin("Assets/Structures/Worlds/Ravencrest_Structure", new Point16(40, 80), Vector2.Zero);
		RavencrestSystem.SpawnNativeNpcs(NPCSpawnTimeframe.WorldGen);

		// Intentionally do NOT force Main.hardMode = false here. ReadCopiedMainWorldData re-syncs hardMode from the main
		// world on every entry, so whatever value gets baked into Ravencrest's .wld is overwritten on load. Forcing it
		// false here would clobber the value set by ReadCopiedMainWorldData on first generation while the player is
		// already in hardmode, leaving Pyra/quests broken for that session.
	}

	public override void Update()
	{
		Main.time++;

		if (Main.dayTime && Main.time >= Main.dayLength)
		{
			Main.dayTime = false;
			Main.time = 0;
		}
		else if (!Main.dayTime && Main.time >= Main.nightLength)
		{
			Main.dayTime = true;
			Main.time = 0;
		}
	}

	public class RavencrestNPC : GlobalNPC 
	{
		public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
		{
			if (SubworldSystem.Current is not RavencrestSubworld)
			{
				return;
			}

			if (Main.invasionType == InvasionID.GoblinArmy && Main.invasionSize > 0)
			{
				return;
			}

			pool.Clear();

			float scoutChance = ModContent.GetInstance<TownScoutNPC>().SpawnChance(spawnInfo);

			if (scoutChance > 0)
			{
				pool.Add(ModContent.NPCType<TownScoutNPC>(), scoutChance);
			}

			if (Main.dayTime)
			{
				pool[NPCID.Bird] = 0.3f;
				pool[NPCID.BirdBlue] = 0.3f;
				pool[NPCID.BirdRed] = 0.3f;
				pool[NPCID.GoldBird] = 0.005f;
			}
			else
			{
				pool[NPCID.Owl] = 0.6f;
			}
		}
	}
}
