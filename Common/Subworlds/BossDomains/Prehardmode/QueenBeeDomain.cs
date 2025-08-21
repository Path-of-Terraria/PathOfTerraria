using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;
using Terraria.Enums;
using Terraria.Localization;
using PathOfTerraria.Common.World.Generation;
using Terraria.DataStructures;
using PathOfTerraria.Common.World.Passes;
using PathOfTerraria.Content.Tiles.BossDomain;
using PathOfTerraria.Content.Projectiles.Utility;
using SubworldLibrary;
using Terraria.GameContent.Tile_Entities;
using PathOfTerraria.Common.World.Generation.Tools;
using PathOfTerraria.Common.Systems.BossTrackingSystems;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Prehardmode;

public class QueenBeeDomain : BossDomainSubworld
{
	public override int Width => 800;
	public override int Height => 1600;
	public override int[] WhitelistedCutTiles => [TileID.BeeHive, TileID.JungleVines, TileID.JunglePlants, TileID.JunglePlants2, ModContent.TileType<RoyalHoneyClumpTile>()];
	public override int[] WhitelistedMiningTiles => [ModContent.TileType<RoyalHoneyClumpTile>()];
	public override (int time, bool isDay) ForceTime => ((int)Main.dayLength / 2, true);

	public FightTracker FightTracker = new([NPCID.QueenBee])
	{
		ResetOnVanish = true,
		HaltTimeOnVanish = 60 * 10,
	};

	public override List<GenPass> Tasks => [
		new PassLegacy("Reset", ResetStep),
		new PassLegacy("Tiles", GenTiles),
		new PassLegacy("Polish", Polish),
		new PassLegacy("Settle Liquids", SettleLiquidsStep.Generation),
		new PassLegacy("AdjustSpawn", AdjustSpawn),
	];

	private void Polish(GenerationProgress progress, GameConfiguration configuration)
	{
		Dictionary<Point, OpenFlags> grasses = [];

		for (int i = 1; i < Width - 1; ++i)
		{
			for (int j = 1; j < Height - 1; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.TileType == TileID.Mud && tile.HasTile)
				{
					OpenFlags flags = OpenExtensions.GetOpenings(i, j, false, false);

					if (flags != OpenFlags.None)
					{
						tile.TileType = TileID.JungleGrass;
						grasses.Add(new Point(i, j), flags);
					}
				}

				if (tile.WallType == WallID.None)
				{
					tile.WallType = WallID.MudUnsafe;
				}

				WorldGen.SquareTileFrame(i, j);
			}
		}

		foreach ((Point grass, OpenFlags flags) in grasses)
		{
			AddGrassDecor(grass.X, grass.Y, flags);
		}

		GenerationUtilities.ManuallyPopulatePlayerSensors();
	}

	private static void AddGrassDecor(int i, int j, OpenFlags flags)
	{
		if (flags.HasFlag(OpenFlags.Above) && !WorldGen.genRand.NextBool(3))
		{
			WorldGen.PlaceTile(i, j - 1, WorldGen.genRand.NextBool(3) ? TileID.JunglePlants2 : TileID.JunglePlants, true, false, style: WorldGen.genRand.Next(24));
		}

		if (flags.HasFlag(OpenFlags.Below))
		{
			if (!WorldGen.genRand.NextBool(4))
			{
				int length = WorldGen.genRand.Next(5, 12);

				for (int k = 1; k < length; ++k)
				{
					if (Main.tile[i, j + k].HasTile)
					{
						break;
					}

					WorldGen.PlaceTile(i, j + k, TileID.JungleVines, true);
				}
			}
			else if (WorldGen.genRand.NextBool(6))
			{
				WorldGen.PlaceTile(i, j + 1, TileID.HangingLanterns, true, false, -1, 13);
			}
		}
	}

	private void GenTiles(GenerationProgress progress, GameConfiguration configuration)
	{
		Main.spawnTileX = Width / 2;
		Main.spawnTileY = (int)(Main.rockLayer + 20); // Puts the player below rockLayer
		Main.worldSurface = 230;
		Main.rockLayer = 299;

		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		for (int x = 0; x < Main.maxTilesX; ++x)
		{
			for (int y = 0; y < Main.maxTilesY; ++y)
			{
				Tile tile = Main.tile[x, y];
				tile.TileType = TileID.Mud;
				tile.HasTile = true;
				tile.WallType = WallID.MudUnsafe;
			}

			progress.Value = (float)x / Main.maxTilesX;
		}
		
		StructureTools.PlaceByOrigin("Assets/Structures/BeeDomain/Arena_" + WorldGen.genRand.Next(2), new Point16(Width / 2, Main.spawnTileY), new(0.5f));

		// Replace hive with unsafe hive wall so it's not destroyed by the tunnel
		for (int i = 1; i < Width - 1; ++i)
		{
			for (int j = 1; j < Height - 1; ++j)
			{
				Tile tile = Main.tile[i, j];

				if (tile.WallType == WallID.Hive)
				{
					tile.WallType = WallID.HiveUnsafe;
				}
			}
		}

		DigTunnel(Width / 4, Main.spawnTileY - WorldGen.genRand.Next(-40, 40), true);
		DigTunnel((int)(Width * 0.75f), Main.spawnTileY - WorldGen.genRand.Next(-40, 40), false);
	}

	private void DigTunnel(int x, int y, bool left)
	{
		var original = new Point16(x, y);

		Vector2[] positions = Tunnel.GeneratePoints([new(x, y), Vector2.Lerp(new(x, y), new(Width / 2, Main.spawnTileY), 0.7f)
			+ new Vector2(WorldGen.genRand.Next(-2, 3), WorldGen.genRand.Next(-2, 3)), new(Width / 2, Main.spawnTileY)], 6, 4, 0.1f);

		FastNoiseLite noise = new(WorldGen._genRandSeed);
		int breakTime = -1;

		foreach (Vector2 pos in positions)
		{
			if (Main.tile[pos.ToPoint()].WallType != WallID.MudUnsafe && breakTime == -1)
			{
				breakTime = 5;
			}

			if (breakTime > -1 && --breakTime == 0) // Makes sure the opening continues a little bit into the hive
			{
				break;
			}

			float mul = 0.8f + MathF.Abs(noise.GetNoise(pos.X, pos.Y)) * 1.2f;
			Digging.CircleOpening(pos, 5 * mul);
			Digging.CircleOpening(pos, WorldGen.genRand.Next(3, 7) * mul);

			if (WorldGen.genRand.NextBool(3, 5))
			{
				WorldGen.digTunnel(pos.X, pos.Y, 0, 0, 5, (int)(WorldGen.genRand.NextFloat(1, 8) * mul));
			}
		}

		StructureTools.PlaceByOrigin($"Assets/Structures/BeeDomain/Mini_{(left ? "" : "R_")}{WorldGen.genRand.Next(4)}", original, new(left ? 1 : 0, 0.5f));
	}

	private static void AdjustSpawn(GenerationProgress progress, GameConfiguration configuration)
	{
		var basePoint = new Point(Main.spawnTileX, Main.spawnTileY);
		var targetSize = new Point(3, 3);
		var searchRadius = new Point(40, 40);

		if (GenerationUtilities.TryFindNearestFreePoint(basePoint, targetSize, searchRadius, out Point spawnPoint))
		{
			(Main.spawnTileX, Main.spawnTileY) = (spawnPoint.X, spawnPoint.Y);
		}
	}

	public override void OnEnter()
	{
		FightTracker.Reset();
	}

	public override void Update()
	{
		Liquid.UpdateLiquid();
		Wiring.UpdateMech();

		TileEntity.UpdateStart();
		foreach (TileEntity te in TileEntity.ByID.Values)
		{
			te.Update();
		}

		TileEntity.UpdateEnd();
		Main.moonPhase = (int)MoonPhase.Full;

		FightState state = FightTracker.UpdateState();

		if (state == FightState.JustCompleted)
		{
			Vector2 pos = new Vector2(Width / 2, Main.spawnTileY - 8) * 16;
			Projectile.NewProjectile(Entity.GetSource_NaturalSpawn(), pos, Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);

			BossTracker.AddDowned(NPCID.QueenBee, false, true);
		}
	}

	public class QueenBeeEdit : ModPlayer
	{
		public override void ResetEffects()
		{
			if (SubworldSystem.Current is QueenBeeDomain)
			{
				Player.ZoneJungle = true;
				Player.ZoneHive = true;
			}
		}
	}
}