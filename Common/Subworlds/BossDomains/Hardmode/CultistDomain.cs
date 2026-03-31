using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.SkeleDomain;
using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Passes;
using PathOfTerraria.Common.World.Utilities;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Items.Placeable.Mapping;
using PathOfTerraria.Content.Tiles.BossDomain;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class CultistDomain : BossDomainSubworld, IOverrideBiome
{
	public const int FloorY = 400;
	public const int PedestalDistance = 400;
	public const int EdgeDistance = 1000;

	private static bool SpawnedLunatic = false;

	public override int Width => 2400;
	public override int Height => 600;
	public override (int time, bool isDay) ForceTime => ((int)(Main.dayLength * 0.95), true);
	public override int[] WhitelistedMiningTiles => [ModContent.TileType<TabletPieces>()];

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new FlatWorldPass(FloorY, tileType: TileID.BlueDungeonBrick),
		new PassLegacy("Structures", GenTerrain),
		new PassLegacy("Convert", DungeonConversion.Convert),
		new PassLegacy("Settle Liquids", SettleLiquidsStep.Generation)];

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Start(1);
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		Point16 cliffSize = StructureTools.GetSize("Assets/Structures/LunaticDomain/CliffLeft");
		int cliffWidth = cliffSize.X;
		int cliffHeight = cliffSize.Y - 1;

		for (int i = 0; i < Width / 2 - EdgeDistance; ++i)
		{
			for (int j = FloorY - cliffHeight; j < Height - 10; ++j)
			{
				Tile tile = Main.tile[i, j];
				tile.HasTile = true;
				tile.TileType = TileID.BlueDungeonBrick;

				tile = Main.tile[Width / 2 + EdgeDistance + i, j];
				tile.HasTile = true;
				tile.TileType = TileID.BlueDungeonBrick;
			}
		}

		StructureTools.PlaceByOrigin("Assets/Structures/LunaticDomain/Center_" + WorldGen.genRand.Next(2), GetFloor(Width / 2, 200), new Vector2(0.5f, 1));

		PriorityQueue<int, float> pieces = new();
		pieces.Enqueue(0, WorldGen.genRand.NextFloat());
		pieces.Enqueue(1, WorldGen.genRand.NextFloat());
		pieces.Enqueue(2, WorldGen.genRand.NextFloat());
		pieces.Enqueue(3, WorldGen.genRand.NextFloat());

		List<int> pieceXs = [];

		PlaceStructureWithPiece("Pedestal_" + WorldGen.genRand.Next(3), Width / 2 - PedestalDistance, new Vector2(0.5f, 1), pieceXs);
		PlaceStructureWithPiece("Pedestal_" + WorldGen.genRand.Next(3), Width / 2 + PedestalDistance, new Vector2(0.5f, 1), pieceXs);
		PlaceStructureWithPiece("CliffLeft", Width / 2 - EdgeDistance, new Vector2(0, 1), pieceXs);
		PlaceStructureWithPiece("CliffRight", Width / 2 + EdgeDistance, new Vector2(1), pieceXs);

		List<int> nonPlacements = [.. pieceXs];
		nonPlacements.Add(Width / 2);
		PlaceBoneyards(nonPlacements);

		OffsetArea();
		ModifyArea();

		foreach (int x in pieceXs)
		{
			PlacePiece(x, pieces.Dequeue());
		}

		Point16 spawn = GetFloor(Width / 2, 20);
		Main.spawnTileX = spawn.X;
		Main.spawnTileY = spawn.Y - 5;
	}

	private static void PlaceBoneyards(List<int> nonPlacements)
	{
		int width = Main.maxTilesX;
		FastNoiseLite noise = new();

		for (int i = 0; i < 2; ++i)
		{
			int x;

			do
			{
				x = WorldGen.genRand.Next(width / 2 - EdgeDistance, width / 2 + EdgeDistance);
			} while (nonPlacements.Any(x2 => Math.Abs(x - x2) < 140));

			int left = x - WorldGen.genRand.Next(55, 100);
			int right = x + WorldGen.genRand.Next(55, 100);

			int leftY = PlaceBoneyardEdge(left, true);
			int rightY = PlaceBoneyardEdge(right, false);

			if (leftY == -1 || rightY == -1)
			{
				i--;
				continue;
			}

			nonPlacements.Add(x);

			List<Point> positions = [];
			float powFactor = WorldGen.genRand.NextFloat(1.5f, 3f);

			for (int placeX = left; placeX < right; ++placeX)
			{
				float factor = MathUtils.Clamp01(Utils.GetLerpValue(left, right, placeX, true) + WorldGen.genRand.NextFloat(-0.015f, 0.015f));
				int y = (int)MathHelper.Lerp(leftY, rightY, factor) + 3;
				int topY = y - 10;
				float halfFactor = 1f - MathF.Pow(Math.Abs(factor - 0.5f) * 2, powFactor);
				int bottomY = y + (int)MathHelper.Lerp(-8, 60, halfFactor);
				int tileY = y + (int)MathHelper.Lerp(-6, 80, halfFactor) + 1;

				for (int placeY = topY; placeY < tileY; ++placeY)
				{
					positions.Add(new Point(placeX, placeY));

					Tile tile = Main.tile[placeX, placeY];
					tile.HasTile = false;

					if (placeY > bottomY)
					{
						tile.HasTile = true;
						tile.TileType = (ushort)(ModContent.TileType<PolishedBone>());
					}
				}

				int boneHeight = (int)(Math.Abs(noise.GetNoise(placeX * 4 + WorldGen.genRand.NextFloat(-1, 1), 0) * 50) * (halfFactor * 2));

				for (int placeY = bottomY - boneHeight + 6; placeY < bottomY + 6; ++placeY)
				{
					Tile tile = Main.tile[placeX, placeY];
					tile.WallType = WallID.Bone;
				}

				float sideMod = Utils.GetLerpValue(0, 0.05f, halfFactor);
				boneHeight = (int)(Math.Abs(noise.GetNoise(placeX * 4 + WorldGen.genRand.NextFloat(-1, 1), 30000) * 50) * ((0.5f - halfFactor) * 2) * sideMod);

				for (int placeY = bottomY - boneHeight + 6; placeY < bottomY + 6; ++placeY)
				{
					Tile tile = Main.tile[placeX, placeY];
					tile.WallType = WallID.Bone;
				}
			}

			foreach (Point pos in positions)
			{
				if (OpenExtensions.GetUnsolidAndWallOpenings(pos.X, pos.Y, false, false) == OpenFlags.None)
				{
					Tile tile = Main.tile[pos];
					tile.WallType = WallID.Bone;
				}
			}
		}
	}

	private static int PlaceBoneyardEdge(int x, bool left)
	{
		if (!WorldUtils.Find(new Point(x, 60), new Searches.Down(Main.maxTilesY - 60).Conditions(new Conditions.IsSolid()), out Point bottom))
		{
			return -1;
		}

		int width = WorldGen.genRand.Next(14, 17);

		if (left)
		{
			bottom.X -= width;
		}

		WorldUtils.Gen(bottom, new Shapes.Rectangle(width, 16), Actions.Chain(new Actions.SetTile((ushort)ModContent.TileType<PolishedBone>()), new NotTouchingAir(), 
			new Actions.PlaceWall(WallID.Bone)));
		return bottom.Y;
	}

	private void OffsetArea()
	{
		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetFrequency(0.004f);
		noise.SetNoiseType(FastNoiseLite.NoiseType.Value);
		noise.SetFractalType(FastNoiseLite.FractalType.FBm);

		for (int i = 2; i < Width - 2; ++i)
		{
			int height = (int)Math.Abs(noise.GetNoise(i * 1.3f, 0) * 16);

			for (int j = 60; j < Height - 60; ++j)
			{
				Tile tile = Main.tile[i, j];
				Tile below = Main.tile[i, j + height];

				tile.HasTile = below.HasTile;
				tile.TileType = below.TileType;
				tile.WallType = below.WallType;
				tile.TileFrameX = below.TileFrameX;
				tile.TileFrameY = below.TileFrameY;
			}
		}
	}

	private void ModifyArea()
	{
		for (int i = 2; i < Width  - 2; ++i)
		{
			for (int j = FloorY - 100; j < Height - 10; ++j)
			{
				Tile tile = Main.tile[i, j];
				
				if (tile.HasTile && tile.TileType == TileID.BlueDungeonBrick && OpenExtensions.GetUnsolidAndWallOpenings(i, j, false, false) == OpenFlags.None)
				{
					tile.WallType = WallID.BlueDungeonUnsafe;
				}

				Tile.SmoothSlope(i, j);
			}
		}

		Main.tileDungeon[TileID.BlueDungeonBrick] = false; // Bypass WorldGen.meteor safety check
		int oldNetMode = Main.netMode;
		Main.netMode = NetmodeID.MultiplayerClient; // Bypass announcement

		for (int i = 10; i < Width - 10; ++i)
		{
			if (WorldGen.genRand.NextBool(600) && Math.Abs(i - Width / 2) > 80)
			{
				int j = GetFloor(i, 200).Y;
				j += WorldGen.genRand.Next(-10, 2);

				bool v = WorldGen.meteor(i, j, true);
			}
		}

		Main.tileDungeon[TileID.BlueDungeonBrick] = true;
		Main.netMode = oldNetMode;

		for (int i = 10; i < Width - 10; ++i)
		{
			int j = GetFloor(i, 200).Y;
			Tile tile = Main.tile[i, j];

			if (tile.TileType == ModContent.TileType<PolishedBone>())
			{
				if (WorldGen.genRand.NextBool(16))
				{
					WorldGen.PlaceObject(i, j - 1, TileID.LargePiles, true, WorldGen.genRand.Next(7));
				}
				else if (WorldGen.genRand.NextBool(8))
				{
					WorldGen.PlaceSmallPile(i, j - 1, WorldGen.genRand.Next(6, 16), 1);
				}
				else if (WorldGen.genRand.NextBool(3))
				{
					WorldGen.PlaceSmallPile(i, j - 1, WorldGen.genRand.Next(11, 28), 0);
				}
			}
			else if (WorldGen.genRand.NextBool(90))
			{
				WorldGen.PlaceObject(i, j - 1, TileID.Lamps, true, 24);
			}
		}
	}

	private static void PlaceStructureWithPiece(string structure, int x, Vector2 origin, List<int> pieces)
	{
		StructureTools.PlaceByOrigin("Assets/Structures/LunaticDomain/" + structure, new Point16(x, FloorY + 1), origin);
		pieces.Add(x);
	}

	private static void PlacePiece(int x, int style)
	{
		while (true)
		{
			int pieceX = x + WorldGen.genRand.Next(-40, 40);
			Point16 pos = GetFloor(pieceX, 200);

			WorldGen.PlaceObject(pos.X, pos.Y - 1, ModContent.TileType<TabletPieces>(), true, style);

			if (Main.tile[pos.X, pos.Y - 1].TileType == ModContent.TileType<TabletPieces>() && Main.tile[pos.X, pos.Y - 1].HasTile)
			{
				return;
			}
		}
	}

	public static Point16 GetFloor(int x, int y)
	{
		while (!WorldGen.SolidOrSlopedTile(x, y))
		{
			y++;
		}

		return new (x, y);
	}

	public override void OnEnter()
	{
		base.OnEnter();
	}

	public override void Update()
	{
		Liquid.UpdateLiquid();

		if (!SpawnedLunatic)
		{
			if (NPC.AnyNPCs(NPCID.CultistBoss))
			{
				SpawnedLunatic = true;
			}
		}
	}

	public void OverrideBiome()
	{
		Main.bgStyle = 0;
		Main.curMusic = MusicID.OverworldDay;
	}
}
