using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Content.Projectiles.Utility;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Biomes;
using Terraria.GameContent.Biomes.CaveHouse;
using Terraria.GameContent.Generation;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.IO;
using Terraria.ObjectData;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class MoonLordDomain : BossDomainSubworld
{
	public const int TerrariaHeight = 1800;

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
		for (int i = 0; i < 35; ++i)
		{
			Point pos = new(WorldGen.genRand.Next(120, Width - 120), Main.rand.Next(TerrariaHeight - 400, TerrariaHeight - 200));

			if (WorldGen.genRand.NextBool(5))
			{
				Cloud(pos.X, pos.Y);
			}
			else if (WorldGen.genRand.NextBool(5))
			{
				WorldGen.CloudLake(pos.X, pos.Y);
			}
			else if (WorldGen.genRand.NextBool(5))
			{
				WorldGen.CloudIsland(pos.X, pos.Y);
			}
			else if (WorldGen.genRand.NextBool(5))
			{
				WorldGen.DesertCloudIsland(pos.X, pos.Y);
			}
			else if (WorldGen.genRand.NextBool(5))
			{
				WorldGen.SnowCloudIsland(pos.X, pos.Y);
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
				tile.TileType = GradientNonsenseTileId(noise, closestValidTileLookup, tileRange, x, y, wallRange, out ushort wallType);
				tile.WallType = wallType;

				TypesUsed.Add(tile.TileType);
			}
		}

		string types = "";

		foreach (int item in TypesUsed)
		{
			types += $"({item}: {TileID.Search.GetName(item)}) ";
		}

		Mod.Logger.Debug("ML DEBUG LIST: " + types);

		progress.Message = "Tunnels";

		Main.spawnTileX = Width / 2;
		Main.spawnTileY = Height - 200;

		WorldUtils.Gen(new Point(Main.spawnTileX, Main.spawnTileY), new Shapes.Circle(12, 8), Actions.Chain(new Modifiers.Blotches(), new Actions.ClearTile(false)));

		for (int i = 0; i < 4; ++i)
		{
			DigTunnel(Main.rand.Next(9, 18));
		}

		progress.Message = "Objects";

		Dictionary<string, int> typeCount = [];

		for (int i = 0; i < 12; ++i)
		{
			bool success = true;

			success &= SpamObject((x, y) => MoonDomainGenerationTools.ForceLivingTree(x, y, false), typeCount, "Tree");
			success &= SpamObject((x, y) => new MarbleBiome().Place(new Point(x, y), GenVars.structures), typeCount, "Marble");
			success &= SpamObject((x, y) => new GraniteBiome().Place(new Point(x + (WorldGen.genRand.NextBool() ? -1 : 1), y), GenVars.structures), typeCount, "Granite", 20);

			success &= SpamObject((x, y) =>
			{
				const int Spacing = 12;
				float xDir = WorldGen.genRand.NextFloat(-Spacing, Spacing);
				float yDir = WorldGen.genRand.NextFloat(-Spacing, Spacing);
				WorldGen.digTunnel(x, y, xDir, yDir, WorldGen.genRand.Next(14, 30), WorldGen.genRand.Next(20, 50));
			}, typeCount, "Tunnel");

			success &= SpamObject((x, y) =>
			{
				var rooms = new Rectangle[WorldGen.genRand.Next(2) + 1];
				rooms[0] = new Rectangle(x, y, 20, 14);

				if (rooms.Length > 1)
				{
					rooms[1] = new Rectangle(x + WorldGen.genRand.Next(-10, 10), y + 14, 20, 14);
				}

				new CaveHouseBiome().Place(new Point(x, y), GenVars.structures);
			}, typeCount, "UGHouse", 12, false);

			if (!success)
			{
				i--;
			}
		}

		progress.Message = "Growth";

		for (int i = 2; i < Width - 2; ++i)
		{
			for (int j = 2400; j < Height - 2; ++j)
			{
				Tile tile = Main.tile[i, j];
				OpenFlags flags = OpenExtensions.GetOpenings(i, j, false, false);

				if (tile.HasTile && tile.TileType == TileID.Dirt && flags != OpenFlags.None)
				{
					tile.TileType = TileID.Grass;
					Decoration.OnPurityGrass(new Point16(i, j), flags, 1);
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
			? WorldGen.genRand.Next((int)MathHelper.Lerp(TerrariaHeight, Height, 0.52f), Height - 150)
			: WorldGen.genRand.Next((int)MathHelper.Lerp(TerrariaHeight, Height, 0.32f), (int)MathHelper.Lerp(TerrariaHeight, Height, 0.48f));

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
			return true;
		}
		else
		{
			return false;
		}
	}

	private void DigTunnel(int baseSize)
	{
		FastNoiseLite noise = new();

		Vector2[] points = Tunnel.GeneratePoints([new(Main.spawnTileX, Main.spawnTileY), new Vector2(RandomX(), StepY(0.2f)),
			new Vector2(RandomX(), StepY(0.3f)), new Vector2(RandomX(), StepY(0.4f)), new Vector2(RandomX(), StepY(0.5f)), new Vector2(RandomX(), StepY(0.6f)),
			new Vector2(RandomX(), StepY(0.7f)), new Vector2(RandomX(), StepY(0.8f)), new Vector2(RandomX(), StepY(0.9f)), new Vector2(RandomX(), StepY(1f))],
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

		float RandomX()
		{
			return WorldGen.genRand.Next(200, Width - 200);
		}
	}

	private static ushort GradientNonsenseTileId(FastNoiseLite noise, Dictionary<int, int> tileLookup, Range tileRange, float x, float y, Range wallRange, out ushort wall)
	{
		const float DirtCutoff = 0.5f;
		const float DirtDitherStart = DirtCutoff + 0.03f;

		const float StoneCutoff = 0.3f;
		const float StoneDitherStart = StoneCutoff + 0.03f;

		float yDistance = (y - TerrariaHeight) / (Main.maxTilesY - TerrariaHeight);

		if (yDistance > DirtCutoff)
		{
			ushort dirt = noise.GetNoise(x, y) < -0.2f ? WallID.Dirt : WallID.Grass;
			wall = yDistance < DirtDitherStart ? Dither(yDistance, DirtCutoff, DirtDitherStart, dirt, WallID.Stone) : dirt;
			return yDistance < DirtDitherStart ? Dither(yDistance, DirtCutoff, DirtDitherStart, TileID.Dirt, TileID.Stone) : TileID.Dirt;
		}
		else if (yDistance > StoneCutoff && WorldGen.genRand.NextFloat() < Utils.GetLerpValue(StoneCutoff, StoneDitherStart, yDistance))
		{
			wall = yDistance < StoneDitherStart ? Dither(yDistance, StoneCutoff, StoneDitherStart, WallID.Stone, WallID.ObsidianBackUnsafe) : WallID.Stone;
			return yDistance < StoneDitherStart ? Dither(yDistance, StoneCutoff, StoneDitherStart, TileID.Stone, TileID.Ash) : TileID.Stone;
		}

		wall = (ushort)MathHelper.Lerp(wallRange.Start.Value, wallRange.End.Value, Utils.GetLerpValue(-1.3f, 0.7f, noise.GetNoise(x * 0.8f, y * 0.8f + 120), true));
		return (ushort)GetNearestTileId(noise.GetNoise(x, y), tileLookup, tileRange);
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
			|| id is TileID.Cactus or TileID.Trees or TileID.EchoBlock or TileID.Boulder or TileID.MetalBars or TileID.Teleporter || TileID.Sets.IsVine[id] 
			|| ModContent.GetModTile(id) is ModTile modTile && modTile.Mod.Name == "ModLoaderMod" || TileID.Sets.Falling[id];
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
