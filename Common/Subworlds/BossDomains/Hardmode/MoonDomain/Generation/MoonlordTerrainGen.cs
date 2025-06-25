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

	private readonly static int[] RandomOreTypes = [TileID.Iron, TileID.Gold, TileID.Lead, TileID.Silver, TileID.Tungsten, TileID.Platinum, TileID.Palladium, 
		TileID.Cobalt, TileID.Mythril, TileID.Orichalcum, TileID.Titanium, TileID.Adamantite];

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

		FastNoiseLite noise = TerrainNoise();
		FastNoiseLite messNoise = TerrainNoise();
		messNoise.SetNoiseType((FastNoiseLite.NoiseType)WorldGen.genRand.Next(6));

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
				tile.TileType = GetTileId(noise, closestValidTileLookup, tileRange, new Vector2(x, y), new Vector2(i, j), wallRange, out ushort wallType, messNoise);
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

		SpamSingleAction((x, y) =>
		{
			const int Embed = 9;

			float angle = OpenExtensions.GetOpenings(x, y, false, false).GetDirectionRandom().ToVector2().ToRotation() + WorldGen.genRand.NextFloat(-0.7f, 0.7f);
			Vector2 offset = angle.ToRotationVector2();
			int id = WorldGen.genRand.NextBool() ? TileID.ShimmerBlock : TileID.ShimmerBrick;

			GenerateSpike(new Point16(x - (int)(offset.X * Embed), y - (int)(offset.Y * Embed)), WorldGen.genRand.Next(20, 40), angle, 0.2f, id);
		}, "Spikes", counts, 20, null);

		SmoothWorld();

		SpamSingleAction((x, y) => WorldGen.TileRunner(x + Main.rand.Next(-40, 40), y + Main.rand.Next(-40, 40), WorldGen.genRand.NextFloat(8, 17),
			WorldGen.genRand.Next(4, 9), WorldGen.genRand.Next(RandomOreTypes)), "Ore", counts, 300);

		SpamTrees(counts);

		while (true)
		{
			bool success = true;

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

			if (success)
			{
				break;
			}
		}

		for (int i = 40; i < Main.maxTilesX - 40; ++i)
		{
			for (int j = MoonLordDomain.CloudBottom; j < MoonLordDomain.CloudBottom + 5; ++j)
			{
				Tile tile = Main.tile[i, j];
				tile.LiquidAmount = 255;
				tile.LiquidType = LiquidID.Water;

				tile = Main.tile[i, j + 167];
				tile.LiquidAmount = 255;
				tile.LiquidType = LiquidID.Lava;

				tile = Main.tile[i, j + 177];
				tile.LiquidAmount = 255;
				tile.LiquidType = LiquidID.Lava;

				tile = Main.tile[i, j + 333];
				tile.LiquidAmount = 255;
				tile.LiquidType = LiquidID.Water;
			}
		}
	}

	public static FastNoiseLite TerrainNoise()
	{
		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
		noise.SetFrequency(0.014f);
		noise.SetFractalType(FastNoiseLite.FractalType.Ridged);
		noise.SetFractalOctaves(2);
		noise.SetFractalGain(-2.570f);
		noise.SetFractalWeightedStrength(-0.066f);
		noise.SetDomainWarpType(FastNoiseLite.DomainWarpType.BasicGrid);
		noise.SetDomainWarpAmp(175);
		return noise;
	}

	private static void SpamSingleAction(Action<int, int> action, string name, Dictionary<string, int> counts, int totalRepeats, bool? location = false)
	{
		while (true)
		{
			bool success = SpamObject(action, counts, name, totalRepeats, location);

			if (success)
			{
				break;
			}
		}
	}

	private static void SpamTrees(Dictionary<string, int> counts)
	{
		while (true)
		{
			bool success = true;

			success &= SpamObject((x, y) => MoonDomainGenerationTools.ForceLivingTree(x, y, WorldGen.genRand.NextBool(3)), counts, "Tree", 6);

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
	}

	private static void SmoothWorld()
	{
		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				if (!WorldGen.genRand.NextBool(3))
				{
					Tile.SmoothSlope(i, j, false);
				}
			}
		}
	}

	internal static FastNoiseLite GetTerrariaNoise()
	{
		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
		noise.SetFrequency(0.014f);
		noise.SetFractalType(FastNoiseLite.FractalType.Ridged);
		noise.SetFractalOctaves(2);
		noise.SetFractalGain(-2.570f);
		noise.SetFractalWeightedStrength(-0.066f);
		noise.SetDomainWarpType(FastNoiseLite.DomainWarpType.BasicGrid);
		noise.SetDomainWarpAmp(175);
		return noise;
	}

	internal static bool SpamObject(Action<int, int> action, Dictionary<string, int> countsByType, string name, int max = 12, bool? dirtArea = true)
	{
		if (countsByType.TryGetValue(name, out int counts) && counts > max)
		{
			return true;
		}

		int y = dirtArea.HasValue ? dirtArea.Value
			? WorldGen.genRand.Next((int)MathHelper.Lerp(MoonLordDomain.TerrariaHeight, Main.maxTilesY, DirtCutoff + 0.02f), Main.maxTilesY - 150)
			: WorldGen.genRand.Next((int)MathHelper.Lerp(MoonLordDomain.TerrariaHeight, Main.maxTilesY, StoneCutoff + 0.02f), 
				(int)MathHelper.Lerp(MoonLordDomain.TerrariaHeight, Main.maxTilesY, DirtCutoff - 0.02f))
			: WorldGen.genRand.Next(MoonLordDomain.TerrariaHeight + 50, (int)MathHelper.Lerp(MoonLordDomain.TerrariaHeight, Main.maxTilesY, StoneCutoff - 0.02f));

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
		const float MaxTunnelSteps = 18f;

		FastNoiseLite noise = new();
		List<Vector2> basePoints = [new(Main.spawnTileX, Main.spawnTileY)];

		for (int i = 1; i < MaxTunnelSteps; ++i)
		{
			basePoints.Add(new Vector2(RandomX(i - 1), StepY(i / (MaxTunnelSteps - 1))));
		}

		Vector2[] points = Tunnel.GeneratePoints([..basePoints], 60, 4, 0.6f);

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
				x = WorldGen.genRand.Next(140, Main.maxTilesX - 140);
			} while (xByTierStep[tier].Any(v => Math.Abs(x - v) < 100));

			xByTierStep[tier].Add(x);
			return x;
		}
	}

	private static ushort GetTileId(FastNoiseLite noise, Dictionary<int, int> lookup, Range tileRange, Vector2 mod, Vector2 real, Range wallRange, out ushort wall, 
		FastNoiseLite messNoise)
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

		if (wall == WallID.EchoWall) // These look ugly
		{
			wall++;
		}

		return (ushort)GetNearestTileId(noise.GetNoise(x, y + 3000) > noise.GetNoise(x - 3000, y) ? noise.GetNoise(x, y) : messNoise.GetNoise(i, j), lookup, tileRange);
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

	public static void GenerateSpike(Point16 anchor, int length, float angle, float angleRange, int tileType, bool replaceTiles = true)
	{
		Rectangle rect = GetSpikeArea(anchor, length, angle, angleRange);
		Vector2 point = anchor.ToVector2() + angle.ToRotationVector2() * length;

		for (int i = rect.X; i < rect.Right; ++i)
		{
			for (int j = rect.Y; j < rect.Bottom; ++j)
			{
				if (Math.Abs(angle - new Vector2(i, j).AngleTo(point)) < angleRange && (replaceTiles || !Main.tile[i, j].HasTile))
				{
					Tile tile = Main.tile[i, j];
					tile.HasTile = true;
					tile.TileType = (ushort)tileType;
				}
			}
		}
	}

	private static Rectangle GetSpikeArea(Point16 anchor, int length, float angle, float angleRange)
	{
		Point topLeft = new(short.MaxValue, short.MaxValue);
		Point bottomRight = Point.Zero;
		Vector2 angleVec = angle.ToRotationVector2();
		Vector2 point = anchor.ToVector2() + angleVec * length;

		ModifyPoints((point + (point.DirectionTo(anchor.ToVector2()) * length * 1.5f).RotatedBy(angleRange)).ToPoint(), ref topLeft, ref bottomRight); // Side 1
		ModifyPoints((point + (point.DirectionTo(anchor.ToVector2()) * length * 1.5f).RotatedBy(-angleRange)).ToPoint(), ref topLeft, ref bottomRight); // Side 2
		ModifyPoints(anchor.ToPoint() + (angleVec * length).ToPoint(), ref topLeft, ref bottomRight); // Point

		return new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);

		static void ModifyPoints(Point point, ref Point topLeft, ref Point bottomRight)
		{
			topLeft.X = Math.Min(topLeft.X, point.X);
			topLeft.Y = Math.Min(topLeft.Y, point.Y);

			bottomRight.X = Math.Max(bottomRight.X, point.X);
			bottomRight.Y = Math.Max(bottomRight.Y, point.Y);
		}
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
			or TileID.TrapdoorClosed or TileID.TrapdoorOpen || TileID.Sets.IsVine[id] || ModContent.GetModTile(id) is ModTile modTile && modTile.Mod.Name == "ModLoaderMod" 
			|| TileID.Sets.Falling[id] || TileID.Sets.TouchDamageImmediate[id] > 0 || TileID.Sets.TouchDamageHot[id];
	}

	private static bool DataHasNoAnchors(TileObjectData data)
	{
		return data.AnchorBottom == AnchorData.Empty && data.AnchorTop == AnchorData.Empty && data.AnchorRight == AnchorData.Empty && data.AnchorLeft == AnchorData.Empty;
	}
}
