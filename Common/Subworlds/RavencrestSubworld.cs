﻿using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Subworlds.RavencrestContent;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Common.Systems.VanillaModifications.BossItemRemovals;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds;

internal class RavencrestSubworld : MappingWorld
{
	public override int Width => 1200;
	public override int Height => 340;
	public override bool ShouldSave => true;
	public override int[] WhitelistedMiningTiles => [TileID.Tombstones];
	public override int[] WhitelistedPlaceableTiles => [TileID.Tombstones];

	public override List<GenPass> Tasks => [new FlatWorldPass(200, true, null, TileID.Dirt, WallID.Dirt), 
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
		WorldGen.shadowOrbSmashed = SubworldSystem.ReadCopiedWorldData<bool>("smashedOrb");
		Main.time = SubworldSystem.ReadCopiedWorldData<double>("time");
		Main.dayTime = SubworldSystem.ReadCopiedWorldData<bool>("dayTime");
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
		Main.spawnTileX = 398;
		Main.spawnTileY = 181;

		WorldFile.LoadWorld_Version2(new BinaryReader(new MemoryStream(PoTMod.Instance.GetFileBytes("Assets/Structures/Worlds/RavencrestSubworld.wld"))));

		foreach (ISpawnInRavencrestNPC npc in ModContent.GetContent<ISpawnInRavencrestNPC>())
		{
			int x = npc.TileSpawn.X * 16;
			int y = npc.TileSpawn.Y * 16;
			NPC.NewNPC(Entity.GetSource_TownSpawn(), x, y, npc.Type);
		}

		Main.hardMode = false;
	}

	public override void Update()
	{
		// Time wasn't being incremented for some reason by default
		Main.time++;
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
		}
	}
}
