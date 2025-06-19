using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Content.Projectiles.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria.DataStructures;
using Terraria.GameContent.Biomes;
using Terraria.GameContent.Biomes.CaveHouse;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ObjectData;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class MoonLordDomain : BossDomainSubworld
{
	public const int TerrariaHeight = 1800;

	// For GetTileId
	const float DirtCutoff = 0.6f;
	const float DirtDitherStart = DirtCutoff + 0.03f;

	const float StoneCutoff = 0.3f;
	const float StoneDitherStart = StoneCutoff + 0.03f;

	public override int Width => 900;
	public override int Height => 4200;
	public override (int time, bool isDay) ForceTime => (3500, false);
	
	private static readonly HashSet<int> TypesUsed = [];

	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", GenerateTerraria),
		new PassLegacy("Clouds", GenerateClouds)];

	private void GenerateClouds(GenerationProgress progress, GameConfiguration configuration)
	{
		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetFrequency(0.04f);
		noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);

		for (int i = 10; i < Width - 10; ++i)
		{
			int topHeight = (int)(noise.GetNoise(i * 0.1f, 0) * 40);
			int botHeight = (int)(noise.GetNoise(i * 0.3f, 500) * 40);

			int top = TerrariaHeight - 320 - topHeight;
			int bottom = TerrariaHeight - 80 + botHeight;

			for (int j = top; j < bottom; ++j)
			{
				Tile tile = Main.tile[i, j];
				float heightFactor = Utils.GetLerpValue(top, bottom, j);
				float value = noise.GetNoise(i, j * 2f);

				if (j < top + 20)
				{
					value = MathHelper.Lerp(value, -0.4f, Math.Abs(j - (top + 20)) / 20f);
				}
				else if (j > bottom - 20)
				{
					value = MathHelper.Lerp(value, -0.4f, Math.Abs(j - (bottom - 20)) / 20f);
				}

				if (value > MathHelper.Lerp(-0.1f, 0.2f, heightFactor))
				{
					tile.HasTile = true;
					tile.TileType = noise.GetNoise(i, j * 2.5f + 2000) > MathHelper.Lerp(-0.4f, 0.2f, heightFactor) ? TileID.RainCloud : TileID.Cloud;
				}
			}
		}
	}

	public static void Cloud(int x, int y)
	{
		int width = WorldGen.genRand.Next(38, 50);
		GenAction action = Actions.Chain(new Modifiers.Blotches(), new Actions.PlaceTile(TileID.Cloud));
		float maxLength = new Vector2(width, 27).Length();

		for (int i = 0; i < 30; ++i)
		{
			Point offset = new(WorldGen.genRand.Next(-width, width), WorldGen.genRand.Next(28));
			float mult = offset.ToVector2().Length() / maxLength;

			if (mult <= 0.18f)
			{
				continue;
			}

			Point pos = new(x + offset.X, y + offset.Y);
			WorldUtils.Gen(pos, new Shapes.Circle((int)(18 * mult), (int)(8 * mult)), action);
		}

		GenAction rainSnowCloudAction = Actions.Chain(new Modifiers.Conditions(new Conditions.IsTile(TileID.RainCloud)), new Modifiers.Blotches(), 
			new Actions.PlaceTile(TileID.Cloud));

		for (int i = 0; i < 8; ++i)
		{
			Point offset = new(x + WorldGen.genRand.Next(-width, width), y + WorldGen.genRand.Next(28));
			int size = 8;

			for (int m = offset.X - size; m <= offset.X + size; m++)
			{
				for (int n = offset.Y - size; n <= offset.Y + size; n++)
				{
					if (n > offset.X)
					{
						double num47 = Math.Abs(m - offset.X);
						double num13 = Math.Abs(n - offset.Y) * 2;

						if (Math.Sqrt(num47 * num47 + num13 * num13) < (size + WorldGen.genRand.Next(2)))
						{
							Tile tile = Main.tile[m, n];
							tile = Main.tile[m, n];
							tile.TileType = TileID.RainCloud;
						}
					}
				}
			}
		}
	}

	private void GenerateTerraria(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Message = "Terrain";

		Main.spawnTileX = Width / 2;
		Main.spawnTileY = Height / 2;
		Main.worldSurface = Height - 50;
		Main.rockLayer = Height - 40;
		TypesUsed.Clear();

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

		for (int i = Math.Max(tileRange.Start.Value - 10, 0); i < Math.Min(tileRange.End.Value + 10, TileID.Count); ++i)
		{
			int id = i;

			if (InvalidId(id))
			{
				SearchForBetterId(ref id);
			}

			closestValidTileLookup.Add(i, id);
		}

		for (int i = 0; i < Width; ++i)
		{
			float x = i;
			float throwawayY = 0;

			noise.DomainWarp(ref x, ref throwawayY);

			for (int j = TerrariaHeight - (int)(noise.GetNoise(x, throwawayY) * 40); j < Height; ++j)
			{
				x = i;
				float y = j;

				noise.DomainWarp(ref x, ref y);

				Tile tile = Main.tile[i, j];
				tile.HasTile = y > TerrariaHeight + 60;
				tile.TileType = GetTileId(noise, closestValidTileLookup, tileRange, new Vector2(x, y), new Vector2(i, j), wallRange, out ushort wallType);
				tile.WallType = wallType;

				TypesUsed.Add(tile.TileType);
			}
		}

		progress.Message = "Tunnels";

		Main.spawnTileX = Width / 2;
		Main.spawnTileY = Height - 200;

		WorldUtils.Gen(new Point(Main.spawnTileX, Main.spawnTileY), new Shapes.Circle(12, 8), Actions.Chain(new Modifiers.Blotches(), new Actions.ClearTile(false)));
		Dictionary<int, List<int>> xByTierStep = [];

		for (int i = 0; i < 4; ++i)
		{
			DigTunnel(Main.rand.Next(9, 18), xByTierStep);
		}

		progress.Message = "Objects";

		Dictionary<string, int> counts = [];

		while (true)
		{
			bool success = true;

			success &= SpamObject((x, y) => MoonDomainGenerationTools.ForceLivingTree(x, y, WorldGen.genRand.NextBool(3)), counts, "Tree", 6);
			success &= SpamObject((x, y) => new MarbleBiome().Place(new Point(x, y), GenVars.structures), counts, "Marble");
			success &= SpamObject((x, y) => new GraniteBiome().Place(new Point(x + (WorldGen.genRand.NextBool() ? -1 : 1), y), GenVars.structures), 
				counts, "Granite", 18, false);

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
			}, counts, "UGHouse", 12, false);

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
			}, counts, "House", 18);

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

		progress.Message = "Growth";

		for (int i = 2; i < Width - 2; ++i)
		{
			for (int j = 2400; j < Height - 2; ++j)
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

	private bool SpamObject(Action<int, int> action, Dictionary<string, int> countsByType, string name, int max = 12, bool dirtArea = true)
	{
		if (countsByType.TryGetValue(name, out int counts) && counts > max)
		{
			return true;
		}

		int y = dirtArea 
			? WorldGen.genRand.Next((int)MathHelper.Lerp(TerrariaHeight, Height, DirtCutoff + 0.02f), Height - 150)
			: WorldGen.genRand.Next((int)MathHelper.Lerp(TerrariaHeight, Height, StoneCutoff + 0.02f), (int)MathHelper.Lerp(TerrariaHeight, Height, DirtCutoff - 0.02f));

		Point pos = new(WorldGen.genRand.Next(200, Width - 200), y);
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

	private void DigTunnel(int baseSize, Dictionary<int, List<int>> xByTierStep)
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
			return MathHelper.Lerp(Main.spawnTileY, TerrariaHeight, factor);
		}

		float RandomX(int tier)
		{
			int x;

			xByTierStep.TryAdd(tier, []);

			do
			{
				x = WorldGen.genRand.Next(200, Width - 200);
			} while (xByTierStep[tier].Any(v => Math.Abs(x - v) < 60));

			xByTierStep[tier].Add(x);
			return x;
		}
	}

	private static ushort GetTileId(FastNoiseLite noise, Dictionary<int, int> lookup, Range tileRange, Vector2 mod, Vector2 real, Range wallRange, out ushort wall)
	{
		(float x, float y) = (mod.X, mod.Y);
		(float i, float j) = (real.X, real.Y);

		float yDistance = (y - TerrariaHeight) / (Main.maxTilesY - TerrariaHeight);

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

	public override void OnEnter()
	{
		base.OnEnter();

		BossSpawned = false;
		ExitSpawned = false;
	}

	public override void Update()
	{
		Main.shimmerAlpha = 1f;
		if (!BossSpawned && NPC.AnyNPCs(NPCID.HallowBoss))
		{
			BossSpawned = true;
		}

		if (BossSpawned && !NPC.AnyNPCs(NPCID.HallowBoss) && !ExitSpawned)
		{
			ExitSpawned = true;

			HashSet<Player> players = [];

			foreach (Player plr in Main.ActivePlayers)
			{
				if (!plr.dead)
				{
					players.Add(plr);
				}
			}

			IEntitySource src = Entity.GetSource_NaturalSpawn();
			Vector2 position = Main.rand.Next([.. players]).Center - new Vector2(0, 60);
			Projectile.NewProjectile(src, position, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
		}
	}
}
