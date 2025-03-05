using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Tiles.BossDomain.Mech;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;

internal class PrimeDomain : BossDomainSubworld
{
	public readonly record struct Hall(Point16 Start, Point16 End, HallwayType HallType);

	public enum HallwayType : byte
	{
		Laser,
		Saw,
		Vice,
		Cannon
	}

	public const int MazeWidth = 11;
	public const int MazeHeight = 7;

	public override int Width => 1200;
	public override int Height => 400;
	public override (int time, bool isDay) ForceTime => ((int)Main.nightLength / 2, false);

	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;
	private static bool LeftSpawn = false;

	/// <summary>
	/// This is used as the check for "everyone's belowground" fails in MP by default. Waiting a bit to do it fixes the issue.
	/// </summary>
	private int _mpDelayTimer = 0;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", GenTerrain),
		new PassLegacy("Tunnels", DigTunnels)];

	private void DigTunnels(GenerationProgress progress, GameConfiguration configuration)
	{
		const int StandardHallSize = 18;

		int spawnX = LeftSpawn ? 120 : Width - 120;
		int spawnY = Height / 2;

		Main.spawnTileX = spawnX;
		Main.spawnTileY = spawnY;

		int startOpeningX = LeftSpawn ? 220 : Width - 220;
		int endOpeningX = !LeftSpawn ? 220 : Width - 220;

		LineCut(spawnX, spawnY - 4, startOpeningX, spawnY - 4, 9);

		for (int i = 0; i < 6; ++i)
		{
			int x = (int)MathHelper.Lerp(startOpeningX, endOpeningX, i / 5f);
			LineCut(x, 90, x, Height - 90, StandardHallSize);
		}

		PriorityQueue<HallwayType, float> types = new(4);
		types.Enqueue(HallwayType.Saw, WorldGen.genRand.NextFloat());
		types.Enqueue(HallwayType.Laser, WorldGen.genRand.NextFloat());
		types.Enqueue(HallwayType.Vice, WorldGen.genRand.NextFloat());
		types.Enqueue(HallwayType.Cannon, WorldGen.genRand.NextFloat());

		List<Hall> halls = [];

		for (int i = 0; i < 4; ++i)
		{
			int y = (Height - 132) / 4 * i;
			LineCut(startOpeningX, 90 + y, endOpeningX, 90 + y, StandardHallSize);

			int lineY = 90 + StandardHallSize / 2;
			halls.Add(new Hall(new Point16(startOpeningX, lineY), new Point16(endOpeningX, lineY), types.Dequeue()));
		}

		foreach (Hall hall in halls)
		{
			GenerateHall(hall);
		}

		string spawn = $"Assets/Structures/SkelePrimeDomain/{(!LeftSpawn ? "Left" : "Right")}Start_" + WorldGen.genRand.Next(3);

		Point16 size = StructureTools.GetSize(spawn);
		StructureTools.PlaceByOrigin(spawn, new Point16(spawnX, spawnY + size.Y % 2), new Vector2(0.5f));
	}

	private void GenerateHall(Hall hall)
	{
		if (hall.HallType == HallwayType.Saw)
		{
			for (int i = 0; i < 30; ++i)
			{
				int x = (int)MathHelper.Lerp(hall.Start.X, hall.End.X, WorldGen.genRand.NextFloat());

				Tile tile = Main.tile[x, hall.End.Y];
				tile.HasTile = true;
				tile.TileType = (ushort)ModContent.TileType<SawAnchor>();

				ModContent.GetInstance<SawAnchor.SawEntity>().Place(x, hall.End.Y);
			}
		}
	}

	private static void LineCut(int spawnX, int spawnY, int endX, int endY, int length)
	{
		bool hori = spawnY == endY;

		if (hori)
		{
			CutHorizontal(spawnX, spawnY, endX, length);
		}
		else
		{
			CutVertical(spawnX, spawnY, endY, length);
		}
	}

	private static void CutVertical(int spawnX, int spawnY, int endY, int length)
	{
		while (spawnY != endY)
		{
			for (int i = spawnX; i < spawnX + length; ++i)
			{
				Tile tile = Main.tile[i, spawnY];
				tile.HasTile = false;
			}

			spawnY += Math.Sign(endY - spawnY);
		}
	}

	private static void CutHorizontal(int spawnX, int spawnY, int endX, int length)
	{
		while (spawnX != endX)
		{
			for (int i = spawnY; i < spawnY + length; ++i)
			{
				Tile tile = Main.tile[spawnX, i];
				tile.HasTile = false;
			}

			spawnX += Math.Sign(endX - spawnX);
		}
	}

	private void GenTerrain(GenerationProgress progress, GameConfiguration configuration)
	{
		progress.Start(1);
		progress.Message = Language.GetTextValue($"Mods.{PoTMod.ModName}.Generation.Terrain");

		LeftSpawn = WorldGen.genRand.NextBool();

		CreateOreNoises(out FastNoiseLite oreNoise, out FastNoiseLite oreTypeNoise);

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 20; ++j)
			{
				Tile tile = Main.tile[i, j];
				tile.TileType = TileID.TinPlating;
				tile.WallType = (ushort)ModContent.WallType<TinPlatingUnsafe>();
				tile.TileColor = PaintID.GrayPaint;
				tile.WallColor = PaintID.GrayPaint;
				tile.HasTile = true;

				if (oreNoise.GetNoise(i, j) > 0.05f)
				{
					int typeNoise = (int)(oreTypeNoise.GetNoise(i, j) * 100 / 25f);

					(ushort type, ushort wall) = typeNoise switch
					{
						0 => (TileID.TinBrick, (ushort)ModContent.WallType<TinBrickUnsafe>()),
						1 => (TileID.LeadBrick, (ushort)ModContent.WallType<IronBrickUnsafe>()),
						2 => (TileID.TungstenBrick, (ushort)ModContent.WallType<SilverBrickUnsafe>()),
						_ => (TileID.CopperPlating, (ushort)ModContent.WallType<PlatinumBrickUnsafe>()),
					};

					tile.TileType = type;
					tile.WallType = wall;

					if (typeNoise == 2)
					{
						tile.TileColor = PaintID.None;
					}

					if (wall != ModContent.WallType<PlatinumBrickUnsafe>() && wall != ModContent.WallType<TinBrickUnsafe>())
					{
						tile.WallColor = PaintID.None;
					}
				}
			}

			progress.Value = (float)i / Main.maxTilesX;
		}
	}

	private static void CreateOreNoises(out FastNoiseLite oreNoise, out FastNoiseLite oreTypeNoise)
	{
		oreNoise = new(WorldGen._genRandSeed);
		oreNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
		oreNoise.SetFrequency(0.04f);
		oreNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
		oreNoise.SetFractalOctaves(4);
		oreNoise.SetFractalLacunarity(5.790f);
		oreNoise.SetFractalGain(0.15f);
		oreNoise.SetFractalWeightedStrength(2.540f);

		oreTypeNoise = new(WorldGen._genRandSeed);
		oreTypeNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
		oreTypeNoise.SetFrequency(0.05f);
		oreTypeNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
		oreTypeNoise.SetFractalOctaves(4);
		oreTypeNoise.SetFractalLacunarity(4.790f);
		oreTypeNoise.SetFractalGain(0.1f);
		oreTypeNoise.SetFractalWeightedStrength(3.140f);
		oreTypeNoise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2Reduced);
		oreTypeNoise.SetDomainWarpAmp(182.500f);
	}

	public override void OnEnter()
	{
		base.OnEnter();

		BossSpawned = false;
		ExitSpawned = false;
	}

	public override void Update()
	{
		Wiring.UpdateMech();
		TileEntity.UpdateStart();

		foreach (TileEntity te in TileEntity.ByID.Values)
		{
			te.Update();
		}

		TileEntity.UpdateEnd();

		if (!BossSpawned)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				_mpDelayTimer = 300;
			}
			else if (Main.netMode == NetmodeID.Server)
			{
				_mpDelayTimer++;
			}

			bool canSpawn = Main.CurrentFrameFlags.ActivePlayersCount > 0 && _mpDelayTimer > 300;
			HashSet<int> who = [];

			if (canSpawn)
			{
				foreach (Player player in Main.ActivePlayers)
				{
				}
			}

			if (canSpawn && Main.CurrentFrameFlags.ActivePlayersCount > 0 && who.Count > 0)
			{
				int plr = Main.rand.Next([.. who]);
				NPC.SpawnOnPlayer(plr, NPCID.SkeletronPrime);

				BossSpawned = true;
			}
		}
		else
		{
			if (!NPC.AnyNPCs(NPCID.Spazmatism) && !NPC.AnyNPCs(NPCID.Retinazer) && !ExitSpawned)
			{
				ExitSpawned = true;

				IEntitySource src = Entity.GetSource_NaturalSpawn();
				//Projectile.NewProjectile(src, new Vector2(Width / 2, BlockLayer - 16).ToWorldCoordinates(), Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
			}
		}
	}
}
