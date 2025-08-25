using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Tiles.BossDomain.Mech;
using System.Collections.Generic;
using System.Linq;
using PathOfTerraria.Common.Systems.BossTrackingSystems;
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
	public override int[] WhitelistedExplodableTiles => [ModContent.TileType<ExplosivePowder>()];
	public override int[] WhitelistedMiningTiles => [ModContent.TileType<GrabberAnchor>()];

	private static bool LeftSpawn = false;
	private static Rectangle Arena = Rectangle.Empty;
	public FightTracker FightTracker = new([NPCID.SkeletronPrime])
	{
		ResetOnVanish = true,
		HaltTimeOnVanish = 60 * 10,
	};

	public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep),
		new PassLegacy("Terrain", GenTerrain),
		new PassLegacy("Tunnels", DigTunnels),
		new PassLegacy("AddDecor", GenDecor)];

	private void GenDecor(GenerationProgress progress, GameConfiguration configuration)
	{
		Dictionary<Point16, OpenFlags> metals = [];

		for (int i = 2; i < Main.maxTilesX - 2; ++i)
		{
			for (int j = 2; j < Main.maxTilesY - 2; ++j)
			{
				Tile tile = Main.tile[i, j];

				OpenFlags flags = OpenExtensions.GetOpenings(i, j, false, false);

				int wall = tile.WallType;

				if (wall != ModContent.WallType<TinBrickUnsafe>() && wall != ModContent.WallType<CopperPlatingUnsafe>() && wall != ModContent.WallType<TinPlatingUnsafe>())
				{
					tile.WallColor = PaintID.None;
				}
				else
				{
					tile.WallColor = PaintID.GrayPaint;
				}

				if (!tile.HasTile || flags == OpenFlags.None)
				{
					continue;
				}

				if (tile.TileType is TileID.TinPlating or TileID.TinBrick or TileID.LeadBrick or TileID.TungstenBrick or TileID.CopperPlating)
				{
					metals.Add(new Point16(i, j), flags);
				}
			}

			progress.Value = (float)i / Main.maxTilesX;
		}

		foreach (KeyValuePair<Point16, OpenFlags> metal in metals)
		{
			TwinsDomain.DecorateMetals(metal.Key, metal.Value, true);

			if (WorldGen.genRand.NextBool(50) && metal.Value.HasCardinal())
			{
				SnakeGemspark(metal.Key, metal.Value);
			}
		}
	}

	private static void SnakeGemspark(Point16 key, OpenFlags value)
	{
		Point direction = value.GetDirectionRandom(WorldGen.genRand);
		direction = new Point(-direction.X, -direction.Y);
		var position = key.ToPoint();
		int count = WorldGen.genRand.Next(10, 50);

		while (WorldGen.SolidOrSlopedTile(position.X, position.Y))
		{
			Tile tile = Main.tile[position];
			tile.WallType = WallID.RubyGemspark;
			tile.WallColor = PaintID.None;

			position += direction;

			if (count-- < 0)
			{
				direction = (OpenFlags.Above | OpenFlags.Below | OpenFlags.Left | OpenFlags.Right).GetDirectionRandom();
				count = WorldGen.genRand.Next(10, 50);
			}
		}
	}

	private void DigTunnels(GenerationProgress progress, GameConfiguration configuration)
	{
		int spawnX = LeftSpawn ? 180 : Width - 180;
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

		Arena = new Rectangle((pos.X + 5) * 16, pos.Y * 16, (arenaSize.X - 10) * 16, arenaSize.Y * 16);
	}

	private static void GenerateHall(Hall hall)
	{
		if (hall.HallType == HallwayType.Saw)
		{
			SpawnHallContent(hall);
			SpawnHallStructures("Assets/Structures/SkelePrimeDomain/SawHall_", hall, 10, 15);
		}
		else if (hall.HallType == HallwayType.Laser)
		{
			SpawnHallContent(hall);
			SpawnHallStructures("Assets/Structures/SkelePrimeDomain/LaserHall_", hall, 10, 12);

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
				offsets.Enqueue(i / 6f, WorldGen.genRand.NextFloat());
			}

			for (int i = -3; i < 3; ++i)
			{
				PlaceLaserTurret(x, y + i * 3, style, offsets.Dequeue());
			}
		}
		else if (hall.HallType == HallwayType.Cannon) // TODO: Fix mp incompatibility
		{
			SpawnHallStructures("Assets/Structures/SkelePrimeDomain/CannonHall_", hall, 8, 6, 1);
		}
		else if (hall.HallType == HallwayType.Vice)
		{
			SpawnHallStructures("Assets/Structures/SkelePrimeDomain/ViceHall_", hall, 10, 15);
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
			}
		}
	}

	private static void SpawnHallStructures(string structureName, Hall hall, int range, int strCount, int widthAdd = 0)
	{
		int repeats = 0;

		for (int i = 0; i < strCount; ++i)
		{
			if (repeats > 30000)
			{
				break;
			}

			int x = (int)MathHelper.Lerp(hall.Start.X, hall.End.X, GenRandom.NextFloat());
			int y = hall.Start.Y;
			string name = structureName + Main.rand.Next(range);
			Point16 size = StructureTools.GetSize(name);
			size = new(size.X + widthAdd, size.Y);

			if (GenVars.structures.CanPlace(new Rectangle(x, y - size.Y / 2, size.X, size.Y), 6) && CanPlaceHallStructureHere(x, y, size))
			{
				Point16 pos = StructureTools.PlaceByOrigin(name, new Point16(x, y), new Vector2(0, 0.5f));
				GenVars.structures.AddProtectedStructure(new Rectangle(pos.X, pos.Y, size.X, size.Y), 6);
			}
			else
			{
				i--;
				repeats++;
			}
		}
	}

	private static bool CanPlaceHallStructureHere(int x, int y, Point16 size)
	{
		const int Height = 11;

		for (int i = x; i < x + size.X - 1; ++i)
		{
			if (!WorldGen.SolidTile(i, y - Height))
			{
				return false;
			}

			if (!WorldGen.SolidTile(i, y + Height - 1))
			{
				return false;
			}
		}

		return true;
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

		FightTracker.Reset();
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

		FightState state = FightTracker.UpdateState();

		if (state == FightState.NotStarted)
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

				int npc = NPC.NewNPC(src, (int)Arena.Center().X, (int)Arena.Center().Y - 25, NPCID.SkeletronPrime);
				Main.npc[npc].GetGlobalNPC<ArenaEnemyNPC>().Arena = true;

				Main.spawnTileX = (int)Arena.Center().X / 16;
				Main.spawnTileY = (int)Arena.Center().Y / 16;

				if (Main.netMode == NetmodeID.Server)
				{
					NetMessage.SendData(MessageID.WorldData);
				}
			}
		}
		else if (state == FightState.JustCompleted)
		{
			//BossTracker.AddDowned(NPCID.SkeletronPrime, false, true);
			IEntitySource src = Entity.GetSource_NaturalSpawn();
			Projectile.NewProjectile(src, Arena.Center() - new Vector2(0, 60), Vector2.Zero, ModContent.ProjectileType<ExitPortal>(), 0, 0, Main.myPlayer);
		}
	}
}
