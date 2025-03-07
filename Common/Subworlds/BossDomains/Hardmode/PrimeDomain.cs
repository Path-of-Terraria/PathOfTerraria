using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Tiles.BossDomain.Mech;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Utilities;
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

	public const int StandardHallSize = 18;

	public static UnifiedRandom GenRandom => Main.rand;

	public override int Width => 1300;
	public override int Height => 400;
	public override (int time, bool isDay) ForceTime => ((int)Main.nightLength / 2, false);

	private static bool BossSpawned = false;
	private static bool ExitSpawned = false;
	private static bool LeftSpawn = false;
	private static Rectangle Arena = Rectangle.Empty;

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", GenTerrain),
		new PassLegacy("Tunnels", DigTunnels)];

	private void DigTunnels(GenerationProgress progress, GameConfiguration configuration)
	{
		int spawnX = LeftSpawn ? 120 : Width - 120;
		int spawnY = Height / 2;

		Main.spawnTileX = spawnX;
		Main.spawnTileY = spawnY;

		int startOpeningX = LeftSpawn ? 280 : Width - 280;
		int endOpeningX = !LeftSpawn ? 280 : Width - 280;

		LineCut(spawnX, spawnY - 4, startOpeningX, spawnY - 4, 9);
		LineCut(endOpeningX, spawnY - 4, !LeftSpawn ? 200 : Width - 200, spawnY - 4, 9);

		for (int i = 0; i < 6; ++i)
		{
			int x = (int)MathHelper.Lerp(startOpeningX, endOpeningX, i / 5f);
			LineCut(x, 90, x, Height - 90, StandardHallSize);
		}

		PriorityQueue<HallwayType, float> types = new(4);
		types.Enqueue(HallwayType.Saw, GenRandom.NextFloat());
		types.Enqueue(HallwayType.Laser, GenRandom.NextFloat());
		types.Enqueue(HallwayType.Vice, GenRandom.NextFloat());
		types.Enqueue(HallwayType.Cannon, GenRandom.NextFloat());

		List<Hall> halls = [];

		for (int i = 0; i < 4; ++i)
		{
			int y = (Height - 132) / 4 * i;
			LineCut(startOpeningX, 90 + y, endOpeningX, 90 + y, StandardHallSize);

			int lineY = 90 + StandardHallSize / 2 + y;
			halls.Add(new Hall(new Point16(startOpeningX, lineY), new Point16(endOpeningX, lineY), types.Dequeue()));
		}

		foreach (Hall hall in halls)
		{
			GenerateHall(hall);
		}

		string spawn = $"Assets/Structures/SkelePrimeDomain/{(!LeftSpawn ? "Left" : "Right")}Start_" + GenRandom.Next(3);
		Point16 size = StructureTools.GetSize(spawn);
		StructureTools.PlaceByOrigin(spawn, new Point16(spawnX, spawnY + size.Y % 2), new Vector2(0.5f));

		string arena = "Assets/Structures/SkelePrimeDomain/Arena_0";
		Point16 arenaSize = StructureTools.GetSize(arena);
		Point16 pos = StructureTools.PlaceByOrigin(arena, new Point16(!LeftSpawn ? 180 : Width - 180, Height / 2 + 2), new Vector2(0.5f));

		Arena = new Rectangle(pos.X * 16, pos.Y * 16, arenaSize.X * 16, arenaSize.Y * 16);
	}

	private static void GenerateHall(Hall hall)
	{
		if (hall.HallType == HallwayType.Saw)
		{
			SpawnHallContent(hall);
			SpawnHallStructures("Assets/Structures/SkelePrimeDomain/SawHall_", hall, 3, 5);
		}
		else if (hall.HallType == HallwayType.Laser)
		{
			SpawnHallContent(hall);

			int x = hall.End.X;
			int y = hall.End.Y;
			int style = 0;

			if (x < Main.maxTilesX / 2)
			{
				x -= 2;
				style = 1;
			}
			else
			{
				x += StandardHallSize;
			}

			PriorityQueue<float, float> offsets = new();

			for (int i = 0; i < 6; ++i)
			{
				offsets.Enqueue(i / 5f, WorldGen.genRand.NextFloat());
			}

			for (int i = -3; i < 3; ++i)
			{
				PlaceLaserTurret(x, y + i * 3, style, offsets.Dequeue());
			}
		}
	}

	private static void PlaceLaserTurret(int x, int y, int style, float timerModifier)
	{
		for (int i = 0; i < 2; ++i)
		{
			for (int j = 0; j < 2; ++j)
			{
				Tile tile = Main.tile[x + i, y + j];
				tile.HasTile = false;
				tile.TileColor = PaintID.None;
			}
		}

		WorldGen.PlaceObject(x, y, ModContent.TileType<LaserShooter>(), style: style);

		int te = ModContent.GetInstance<LaserShooter.LaserShooterTE>().Place(x, y);
		(TileEntity.ByID[te] as LaserShooter.LaserShooterTE).Timer = (int)(LaserShooter.LaserShooterTE.MaxTimer * timerModifier);
	}

	public static void SpawnHallContent(Hall hall)
	{
		int start = Math.Min(hall.Start.X, hall.End.X);
		int end = Math.Max(hall.Start.X, hall.End.X);

		for (int x = start; x < end; ++x)
		{
			for (int y = hall.Start.Y - StandardHallSize / 2 - 3; y < hall.Start.Y + StandardHallSize / 2 + 3; ++y)
			{
				if (!WorldGen.SolidTile(x, y))
				{
					continue;
				}

				OpenFlags flags = OpenExtensions.GetOpenings(x, y, false, false);
				string binary = Convert.ToString((byte)flags, 2);
				int count = binary.ToCharArray().Count('1');

				if (count >= 5 && hall.HallType == HallwayType.Saw)
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = (ushort)ModContent.TileType<SawAnchor>();

					ModContent.GetInstance<SawAnchor.SawEntity>().Place(x, y);
				}

				if (count == 3 && hall.HallType == HallwayType.Laser && WorldGen.genRand.NextBool(30))
				{
					Tile tile = Main.tile[x, y];
					tile.HasTile = true;
					tile.TileType = (ushort)ModContent.TileType<ConstantLaser>();
					tile.TileFrameX = (short)(y < hall.Start.Y ? 18 : 0);
					tile.TileFrameY = 0;

					ModContent.GetInstance<ConstantLaser.ConstantLaserTE>().Place(x, y);
				}
			}
		}
	}

	private static void SpawnHallStructures(string structureName, Hall hall, int range, int strCount)
	{
		for (int i = 0; i < strCount; ++i)
		{
			int x = (int)MathHelper.Lerp(hall.Start.X, hall.End.X, GenRandom.NextFloat());
			int y = hall.Start.Y;
			string name = structureName + Main.rand.Next(range);

			StructureTools.PlaceByOrigin(name, new Point16(x, y), new Vector2(0, 0.5f));
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

		LeftSpawn = GenRandom.NextBool();

		CreateOreNoises(out FastNoiseLite oreNoise, out FastNoiseLite oreTypeNoise);

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 10; ++j)
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
						1 => (TileID.LeadBrick, (ushort)ModContent.WallType<LeadBrickUnsafe>()),
						2 => (TileID.TungstenBrick, (ushort)ModContent.WallType<TungstenBrickUnsafe>()),
						_ => (TileID.CopperPlating, (ushort)ModContent.WallType<CopperPlatingUnsafe>()), 
					};

					tile.TileType = type;
					tile.WallType = wall;

					if (wall != ModContent.WallType<TinBrickUnsafe>() && wall != ModContent.WallType<CopperPlatingUnsafe>())
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
			bool canSpawn = Main.CurrentFrameFlags.ActivePlayersCount > 0;
			HashSet<int> who = [];

			if (canSpawn)
			{
				foreach (Player player in Main.ActivePlayers)
				{
					if (!Arena.Intersects(player.Hitbox))
					{
						canSpawn = false;
						break;
					}
					else
					{
						who.Add(player.whoAmI);
					}
				}
			}

			if (canSpawn && Main.CurrentFrameFlags.ActivePlayersCount > 0 && who.Count > 0)
			{
				int plr = Main.rand.Next([.. who]);
				IEntitySource src = Entity.GetSource_NaturalSpawn();
				NPC.NewNPC(src, (int)Arena.Center().X, (int)Arena.Center().Y - 25, NPCID.SkeletronPrime);

				Main.spawnTileX = (int)Arena.Center().X / 16;
				Main.spawnTileY = (int)Arena.Center().Y / 16;
				
				BossSpawned = true;
			}
		}
		else
		{
			if (!NPC.AnyNPCs(NPCID.SkeletronPrime) && !ExitSpawned)
			{
				ExitSpawned = true;

				IEntitySource src = Entity.GetSource_NaturalSpawn();
				Projectile.NewProjectile(src, Arena.Center() - new Vector2(0, 60), Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
			}
		}
	}
}
