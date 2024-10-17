using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Systems.DisableBuilding;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Content.NPCs.Town;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds;

internal class RavencrestSubworld : MappingWorld
{
	public override int Width => 600;
	public override int Height => 400;
	public override bool ShouldSave => true;

	public override List<GenPass> Tasks => [new FlatWorldPass(200, true, null, TileID.Dirt, WallID.Dirt), new PassLegacy("World", SpawnWorld),
		new PassLegacy("Grass", GrowGrass)];

	private void GrowGrass(GenerationProgress progress, GameConfiguration configuration)
	{
		Dictionary<Point16, OpenFlags> tiles = [];
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingWorld");

		for (int i = 0; i < Main.maxTilesX; ++i)
		{
			for (int j = 80; j < Main.maxTilesY - 50; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (!tile.HasTile || tile.TileType != TileID.Dirt)
				{
					continue;
				}

				OpenFlags flags = OpenExtensions.GetOpenings(i, j);

				if (flags == OpenFlags.None)
				{
					continue;
				}

				tiles.Add(new Point16(i, j), flags);
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

		HashSet<Point16> grasses = [];

		foreach ((Point16 position, OpenFlags tile) in tiles)
		{
			Decoration.GrowGrass(position, grasses);
		}

		foreach (Point16 position in grasses)
		{
			Decoration.OnPurityGrass(position);
		}
	}

	public override void CopyMainWorldData()
	{
		SubworldSystem.CopyWorldData("smashedOrb", WorldGen.shadowOrbSmashed); // Copies this bool over since TownScoutNPC needs this
	}

	public override void ReadCopiedMainWorldData()
	{
		WorldGen.shadowOrbSmashed = SubworldSystem.ReadCopiedWorldData<bool>("smashedOrb");
	}

	private void SpawnWorld(GenerationProgress progress, GameConfiguration configuration)
	{
		StructureTools.PlaceByOrigin("Assets/Structures/Ravencrest", new Point16(Width / 2, 188), new(0.5f));
		Main.spawnTileX = Width / 2;
		Main.spawnTileY = 188;

		NPC.NewNPC(Entity.GetSource_TownSpawn(), Main.maxTilesX * 8, Main.maxTilesY * 8 - 200, ModContent.NPCType<BlacksmithNPC>());
		NPC.NewNPC(Entity.GetSource_TownSpawn(), Main.maxTilesX * 8, Main.maxTilesY * 8 - 200, ModContent.NPCType<HunterNPC>());
		NPC.NewNPC(Entity.GetSource_TownSpawn(), Main.maxTilesX * 8, Main.maxTilesY * 8 - 200, ModContent.NPCType<WizardNPC>());
	}

	public class RavencrestNPC : GlobalNPC 
	{
		public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
		{
			if (Main.invasionType == InvasionID.GoblinArmy && Main.invasionSize > 0)
			{
				return;
			}

			pool.Clear();
			pool.Add(ModContent.NPCType<TownScoutNPC>(), ModContent.GetInstance<TownScoutNPC>().SpawnChance(spawnInfo));
		}
	}
}
