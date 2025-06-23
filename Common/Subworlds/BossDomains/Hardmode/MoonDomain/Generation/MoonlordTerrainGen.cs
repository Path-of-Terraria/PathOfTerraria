using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Common.World.Generation;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Biomes.CaveHouse;
using Terraria.GameContent.Biomes;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain.Generation;
internal class MoonlordTerrainGen
{
	public const float DirtCutoff = 0.6f;
	public const float DirtDitherStart = DirtCutoff + 0.03f;
	public const float StoneCutoff = 0.3f;
	public const float StoneDitherStart = StoneCutoff + 0.03f;

#pragma warning disable IDE0060 // Remove unused parameter
	public static void GenerateTerraria(GenerationProgress progress, GameConfiguration configuration)
#pragma warning restore IDE0060 // Remove unused parameter
	{
		progress.Start(1);
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Scanning");

		Main.spawnTileX = Main.maxTilesX / 2;
		Main.spawnTileY = Main.maxTilesY / 2;
		Main.worldSurface = Main.maxTilesY - 50;
		Main.rockLayer = Main.maxTilesY - 40;

		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
		noise.SetFrequency(0.014f);
		noise.SetFractalType(FastNoiseLite.FractalType.Ridged);
		noise.SetFractalOctaves(2);
		noise.SetFractalGain(-2.570f);
		noise.SetFractalWeightedStrength(-0.066f);
		noise.SetDomainWarpType(FastNoiseLite.DomainWarpType.BasicGrid);
		noise.SetDomainWarpAmp(175);

		const int TileRangeStep = 30;

		// Tile ID info
		Dictionary<int, int> closestValidTileLookup = [];
		int rangeSpace = TileID.Count / TileRangeStep;
		int baseRange = Main.rand.Next(rangeSpace + 15, rangeSpace * (TileRangeStep - 1) - 15);
		Range tileRange = (baseRange - rangeSpace)..(baseRange + rangeSpace);

		rangeSpace = WallLoader.WallCount / TileRangeStep;
		baseRange = Main.rand.Next(rangeSpace + 15, rangeSpace * (TileRangeStep - 1) - 15);
		Range wallRange = (baseRange - rangeSpace)..(baseRange + rangeSpace);

		int scanStart = Math.Max(tileRange.Start.Value - 10, 0);
		int scanEnd = Math.Min(tileRange.End.Value + 10, TileID.Count);

		for (int i = scanStart; i < scanEnd; ++i)
		{
			int id = i;

			if (InvalidId(id))
			{
				SearchForBetterId(ref id);
			}

			closestValidTileLookup.Add(i, id);

			progress.Set(Utils.GetLerpValue(scanStart, scanEnd, i));
		}

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		for (int i = 0; i < Main.maxTilesX; ++i)
		{
			float x = i;
			float throwawayY = 0;

			noise.DomainWarp(ref x, ref throwawayY);

			for (int j = MoonLordDomain.TerrariaHeight - (int)(noise.GetNoise(x, throwawayY) * 40); j < Main.maxTilesY; ++j)
			{
				x = i;
				float y = j;

				noise.DomainWarp(ref x, ref y);

				Tile tile = Main.tile[i, j];
				tile.HasTile = y > MoonLordDomain.TerrariaHeight + 60;
				tile.TileType = GetTileId(noise, closestValidTileLookup, tileRange, new Vector2(x, y), new Vector2(i, j), wallRange, out ushort wallType);
				tile.WallType = wallType;
			}

			progress.Set(i / (float)Main.maxTilesX);
		}

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Tunnels");

		Main.spawnTileX = Main.maxTilesX / 2;
		Main.spawnTileY = Main.maxTilesY - 200;

		WorldUtils.Gen(new Point(Main.spawnTileX, Main.spawnTileY), new Shapes.Circle(12, 8), Actions.Chain(new Modifiers.Blotches(), new Actions.ClearTile(false)));
		Dictionary<int, List<int>> xByTierStep = [];

		for (int i = 0; i < 4; ++i)
		{
			DigTunnel(Main.rand.Next(9, 18), xByTierStep);
			progress.Set(i / 3f);
		}

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.PopulatingWorld");

		Dictionary<string, int> counts = [];

		while (true)
		{
			bool success = true;

			success &= SpamObject((x, y) => MoonDomainGenerationTools.ForceLivingTree(x, y, WorldGen.genRand.NextBool(3)), counts, "Tree", 6);
			success &= SpamObject((x, y) => new MarbleBiome().Place(new Point(x, y), GenVars.structures), counts, "Marble", 5);
			success &= SpamObject((x, y) =>
			{
				Tile tile = Main.tile[x, y];
				tile.HasTile = false;

				new GraniteBiome().Place(new Point(x, y), GenVars.structures);
			}, counts, "Granite", 7, false);

			success &= SpamObject((x, y) =>
			{
				HouseBuilder builder = MoonDomainGenerationTools.CreateHouseBuilder(new Point(x, y), WorldGen.genRand.Next(3) switch
				{
					0 => HouseType.Ice,
					1 => HouseType.Granite,
					_ => HouseType.Marble,
				});

				if (builder.IsValid)
				{
					builder.ChestChance = 0;
					builder.Place(new HouseBuilderContext(), GenVars.structures);
				}
			}, counts, "UGHouse", 16, false);

			success &= SpamObject((x, y) =>
			{
				HouseBuilder builder = MoonDomainGenerationTools.CreateHouseBuilder(new Point(x, y), WorldGen.genRand.Next(3) switch
				{
					0 => HouseType.Jungle,
					1 => HouseType.Mushroom,
					_ => HouseType.Wood,
				});

				if (builder.IsValid)
				{
					builder.ChestChance = 0;
					builder.Place(new HouseBuilderContext(), GenVars.structures);
				}
			}, counts, "House", 16);

			success &= SpamObject((x, y) => WorldGen.TileRunner(x + Main.rand.Next(-40, 40), y + Main.rand.Next(-40, 40), WorldGen.genRand.NextFloat(8, 17),
				WorldGen.genRand.Next(4, 9), WorldGen.genRand.Next(12) switch
				{
					0 => TileID.Iron,
					1 => TileID.Gold,
					2 => TileID.Lead,
					3 => TileID.Silver,
					4 => TileID.Tungsten,
					5 => TileID.Platinum,
					6 => TileID.Palladium,
					7 => TileID.Cobalt,
					8 => TileID.Mythril,
					9 => TileID.Orichalcum,
					10 => TileID.Titanium,
					_ => TileID.Adamantite,
				}), counts, "Ore", 300, false);

			success &= SpamObject((x, y) =>
			{
				int height = WorldGen.genRand.Next(20, 80);
				MoonDomainGenerationTools.GenerateMahoganyTree(new Point(x, y), new Point(x, y - height));
			}, counts, "MahoganyTree", 6);

			if (success)
			{
				break;
			}
		}

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Growing");

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2400; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];
				OpenFlags flags = OpenExtensions.GetOpenings(i, j, false, false);

				if (tile.WallType == WallID.None)
				{
					tile.WallType = WallID.LunarRustBrickWall;
				}

				if (!tile.HasTile)
				{
					continue;
				}

				if (tile.TileType == TileID.Dirt && flags != OpenFlags.None)
				{
					tile.TileType = TileID.Grass;
					Decoration.OnPurityGrass(new Point16(i, j), flags, 1);
				}
				else if (tile.TileType == TileID.Stone && flags != OpenFlags.None)
				{
					tile.TileType = noise.GetNoise(i, j) switch
					{
						< -0.4f => TileID.ArgonMoss,
						< -0.2f => TileID.LavaMoss,
						< 0f => TileID.VioletMoss,
						< 0.2f => TileID.XenonMoss,
						< 0.4f => TileID.KryptonMoss,
						_ => TileID.Stone,
					};
				}
			}
		}
	}

	internal static bool SpamObject(Action<int, int> action, Dictionary<string, int> countsByType, string name, int max = 12, bool dirtArea = true)
	{
		if (countsByType.TryGetValue(name, out int counts) && counts > max)
		{
			return true;
		}

		int y = dirtArea
			? WorldGen.genRand.Next((int)MathHelper.Lerp(MoonLordDomain.TerrariaHeight, Main.maxTilesY, DirtCutoff + 0.02f), Main.maxTilesY - 150)
			: WorldGen.genRand.Next((int)MathHelper.Lerp(MoonLordDomain.TerrariaHeight, Main.maxTilesY, StoneCutoff + 0.02f), 
				(int)MathHelper.Lerp(MoonLordDomain.TerrariaHeight, Main.maxTilesY, DirtCutoff - 0.02f));

		Point pos = new(WorldGen.genRand.Next(200, Main.maxTilesX - 200), y);
		OpenFlags flags = OpenExtensions.GetOpenings(pos.X, pos.Y);

		if (flags != OpenFlags.None && Main.tile[pos].HasTile)
		{
			action(pos.X, pos.Y);

			if (!countsByType.TryGetValue(name, out int value))
			{
				countsByType.Add(name, 0);
			}

			countsByType[name] = ++value;
			return false;
		}
		else
		{
			return false;
		}
	}

	internal static void DigTunnel(int baseSize, Dictionary<int, List<int>> xByTierStep)
	{
		FastNoiseLite noise = new();

		Vector2[] points = Tunnel.GeneratePoints([new(Main.spawnTileX, Main.spawnTileY), new Vector2(RandomX(0), StepY(0.1f)), new Vector2(RandomX(-1), StepY(0.15f)),
			new Vector2(RandomX(1), StepY(0.2f)), new Vector2(RandomX(2), StepY(0.3f)), new Vector2(RandomX(3), StepY(0.4f)), new Vector2(RandomX(4), StepY(0.5f)),
			new Vector2(RandomX(5), StepY(0.55f)), new Vector2(RandomX(6), StepY(0.6f)), new Vector2(RandomX(7), StepY(0.7f)), new Vector2(RandomX(8), StepY(0.75f)),
			new Vector2(RandomX(9), StepY(0.8f)), new Vector2(RandomX(10), StepY(0.85f)), new Vector2(RandomX(11), StepY(0.9f)), new Vector2(RandomX(12), StepY(1f))],
			60, 4, 0.6f);

		foreach (Vector2 point in points)
		{
			int size = baseSize + (int)(noise.GetNoise(point.X, point.Y) * 6);
			WorldUtils.Gen(point.ToPoint(), new Shapes.Circle(size, size), new Actions.ClearTile(false));
		}

		return;

		float StepY(float factor)
		{
			return MathHelper.Lerp(Main.spawnTileY, MoonLordDomain.TerrariaHeight, factor);
		}

		float RandomX(int tier)
		{
			int x;

			xByTierStep.TryAdd(tier, []);

			do
			{
				x = WorldGen.genRand.Next(200, Main.maxTilesX - 200);
			} while (xByTierStep[tier].Any(v => Math.Abs(x - v) < 60));

			xByTierStep[tier].Add(x);
			return x;
		}
	}

	private static ushort GetTileId(FastNoiseLite noise, Dictionary<int, int> lookup, Range tileRange, Vector2 mod, Vector2 real, Range wallRange, out ushort wall)
	{
		(float x, float y) = (mod.X, mod.Y);
		(float i, float j) = (real.X, real.Y);

		float yDistance = (y - MoonLordDomain.TerrariaHeight) / (Main.maxTilesY - MoonLordDomain.TerrariaHeight);

		if (yDistance > DirtCutoff)
		{
			ushort dirt = noise.GetNoise(i, j) < -0.2f ? WallID.Dirt : WallID.Grass;
			bool ice = noise.GetNoise(i * 0.4f, j * 0.4f) < -0.3f;
			int stone = ice ? (noise.GetNoise(x * 0.4f, (y + 3000) * 0.4f) > 0.2f ? TileID.IceBlock : TileID.SnowBlock) : TileID.Stone;

			wall = yDistance < DirtDitherStart ? Dither(yDistance, DirtCutoff, DirtDitherStart, dirt, ice ? WallID.IceUnsafe : WallID.Stone) : dirt;
			return yDistance < DirtDitherStart ? Dither(yDistance, DirtCutoff, DirtDitherStart, TileID.Dirt, (ushort)stone) : TileID.Dirt;
		}
		else if (yDistance > StoneCutoff && WorldGen.genRand.NextFloat() < Utils.GetLerpValue(StoneCutoff, StoneDitherStart, yDistance))
		{
			bool ice = noise.GetNoise(i * 0.4f, j * 0.4f) < -0.3f;
			int stone = ice ? (noise.GetNoise(x * 0.4f, (y + 3000) * 0.4f) > 0.2f ? TileID.IceBlock : TileID.SnowBlock) : TileID.Stone;
			wall = ice ? WallID.IceUnsafe : WallID.Stone;
			return (ushort)stone;
		}

		wall = (ushort)MathHelper.Lerp(wallRange.Start.Value, wallRange.End.Value, Utils.GetLerpValue(-1.3f, 0.7f, noise.GetNoise(x * 0.8f, y * 0.8f + 120), true));
		return (ushort)GetNearestTileId(noise.GetNoise(x, y), lookup, tileRange);
	}

	private static ushort Dither(float yDistance, float min, float max, ushort bottom, ushort top)
	{
		return WorldGen.genRand.NextFloat() < Utils.GetLerpValue(min, max, yDistance) ? bottom : top;
	}

	private static int GetNearestTileId(float noise, Dictionary<int, int> closestValidTileLookup, Range idRange)
	{
		int id = (int)MathHelper.Lerp(idRange.Start.Value, idRange.End.Value, Utils.GetLerpValue(-1.3f, 0.7f, noise, true));
		return closestValidTileLookup[id];
	}

	private static void SearchForBetterId(ref int id)
	{
		int topId = id;

		while (InvalidId(topId))
		{
			if (topId > TileID.Count - 1)
			{
				topId = -1;
				break;
			}

			topId++;
		}

		int bottomId = id;

		while (InvalidId(bottomId))
		{
			bottomId--;

			if (bottomId == -1)
			{
				break;
			}
		}

		if (topId == -1 || Math.Abs(id - bottomId) < Math.Abs(id - topId))
		{
			id = bottomId;
		}
		else
		{
			id = topId;
		}
	}

	public static bool InvalidId(int id)
	{
		var data = TileObjectData.GetTileData(id, 0);
		return (data == null || DataHasNoAnchors(data)) && Main.tileFrameImportant[id] || Main.tileCut[id] || id < TileID.Count && !Main.tileSolid[id]
			|| id is TileID.Cactus or TileID.Trees or TileID.EchoBlock or TileID.Boulder or TileID.MetalBars or TileID.Teleporter or TileID.TallGateClosed
			|| TileID.Sets.IsVine[id] || ModContent.GetModTile(id) is ModTile modTile && modTile.Mod.Name == "ModLoaderMod" || TileID.Sets.Falling[id];
	}

	private static bool DataHasNoAnchors(TileObjectData data)
	{
		return data.AnchorBottom == AnchorData.Empty && data.AnchorTop == AnchorData.Empty && data.AnchorRight == AnchorData.Empty && data.AnchorLeft == AnchorData.Empty;
	}
}
