using PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode.SkeleDomain;
using PathOfTerraria.Common.Subworlds.Passes;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Common.World.Passes;
using PathOfTerraria.Content.Tiles.BossDomain;
using System.Collections.Generic;
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

		PlaceStructureWithPiece("Pedestal_" + WorldGen.genRand.Next(2), Width / 2 - PedestalDistance, new Vector2(0.5f, 1), pieceXs);
		PlaceStructureWithPiece("Pedestal_" + WorldGen.genRand.Next(2), Width / 2 + PedestalDistance, new Vector2(0.5f, 1), pieceXs);
		PlaceStructureWithPiece("CliffLeft", Width / 2 - EdgeDistance, new Vector2(0, 1), pieceXs);
		PlaceStructureWithPiece("CliffRight", Width / 2 + EdgeDistance, new Vector2(1), pieceXs);

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

	private void OffsetArea()
	{
		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetFrequency(0.004f);
		noise.SetNoiseType(FastNoiseLite.NoiseType.Value);
		noise.SetFractalType(FastNoiseLite.FractalType.FBm);

		for (int i = 2; i < Width - 2; ++i)
		{
			int height = (int)Math.Abs(noise.GetNoise(i, 0) * 16);

			for (int j = 60; j < Height - 60; ++j)
			{
				Tile tile = Main.tile[i, j];
				Tile below = Main.tile[i, j + height];

				tile.HasTile = below.HasTile;
				tile.TileType = below.TileType;
				tile.WallType = below.WallType;
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
			if (WorldGen.genRand.NextBool(30))
			{
				int j = GetFloor(i, 200).Y;
				j += WorldGen.genRand.Next(-10, 2);

				float xVel = WorldGen.genRand.NextFloat(-2f, 2);
				float yVel = WorldGen.genRand.NextFloat(-0.5f, 2);
				WorldGen.digTunnel(i, j, xVel, yVel, WorldGen.genRand.Next(4, 20), WorldGen.genRand.Next(2, 6), WorldGen.genRand.NextBool(8));
			}
			else if (WorldGen.genRand.NextBool(600) && Math.Abs(i - Width / 2) > 80)
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
			if (WorldGen.genRand.NextBool(90))
			{
				int j = GetFloor(i, 200).Y;

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
