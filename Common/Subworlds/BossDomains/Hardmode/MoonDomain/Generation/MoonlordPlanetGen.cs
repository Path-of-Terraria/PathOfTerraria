using PathOfTerraria.Common.Systems.MiscUtilities;
using PathOfTerraria.Common.Utilities.Extensions;
using PathOfTerraria.Common.World.Generation;
using PathOfTerraria.Content.Tiles.BossDomain.Moon;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain.Generation;

internal static class MoonlordPlanetGen
{
	public readonly struct PlanetTileSet(int Main, int Alt, int Sub, int SubAlt, int WallMain, int WallAlt)
	{
		public readonly int Main = Main;
		public readonly int Alt = Alt;
		public readonly int Sub = Sub;
		public readonly int SubAlt = SubAlt;
		public readonly int WallMain = WallMain;
		public readonly int WallAlt = WallAlt;

		public bool ContainsTile(int type)
		{
			return type == Main || type == Alt || type == Sub || type == SubAlt;
		}
	}

	public readonly record struct PlanetInstance(Vector2 Position, int Radius);

	public static PlanetTileSet Solar = new(TileID.LunarBlockSolar, TileID.SolarBrick, TileID.Hellstone, TileID.Obsidian, WallID.SolarBrick, WallID.HellstoneBrickUnsafe);
	public static PlanetTileSet Nebula = new(TileID.LunarBlockNebula, TileID.NebulaBrick, TileID.VioletMossBlock, TileID.CrystalBlock, WallID.NebulaBrick, WallID.Crystal);
	public static PlanetTileSet Vortex = new(TileID.LunarBlockVortex, TileID.VortexBrick, TileID.MarbleBlock, TileID.BlueDungeonBrick, WallID.VortexBrick, 
		WallID.CryocoreBrickWall);
	public static PlanetTileSet Stardust = new(TileID.LunarBlockStardust, TileID.StardustBrick, TileID.Sunplate, TileID.GoldStarryGlassBlock, WallID.StardustBrick, 
		WallID.BlueStarryGlassWall);

	public enum PlanetType
	{
		Vortex,
		Stardust,
		Solar,
		Nebula
	}

	internal static void GeneratePlanet(PlanetType type, List<PlanetInstance> points)
	{
		FastNoiseLite noise = new(WorldGen._genRandSeed);
		int tries = 0;
		int x;
		int y;

		do
		{
			x = WorldGen.genRand.Next(190, Main.maxTilesX - 190);
			y = WorldGen.genRand.Next(MoonLordDomain.PlanetTop, MoonLordDomain.CloudTop - 200);

			if (++tries > 30000)
			{
				break;
			}
		} while (points.Any(v => new Vector2(x, y).DistanceSQ(v.Position) < MathF.Pow(v.Radius + 100, 2)));

		if (tries > 30000)
		{
			return;
		}

		FastNoiseLite valueNoise = new(WorldGen._genRandSeed);
		valueNoise.SetNoiseType(FastNoiseLite.NoiseType.Value);
		valueNoise.SetFrequency(0.08f);

		FastNoiseLite cellularNoise = new(WorldGen._genRandSeed);
		cellularNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
		cellularNoise.SetFrequency(0.055f);

		GetDataBasedOnType(type, out PlanetTileSet tileSet, out int size, out int gasType);

		size = (int)(size * WorldGen.genRand.NextFloat(0.7f, 1.2f));

		if (type == PlanetType.Nebula)
		{
			size += 8; // Add buffer for gas outline
		}

		Dictionary<Point16, float> nebulaGasLocations = [];

		points.Add(new PlanetInstance(new Vector2(x, y), size));

		GeneratePlanetInstance(type, noise, x, y, valueNoise, cellularNoise, tileSet, size, gasType, nebulaGasLocations);
		AddGasses(type, gasType, new Point16(x, y), size);
		GenerateNebulaGasRing(gasType, nebulaGasLocations);

		if (y < MoonLordDomain.PlanetTop + size * 2)
		{
			GenerateLuminiteSteps(x, y, size, valueNoise);
		}
	}

	private static void GetDataBasedOnType(PlanetType type, out PlanetTileSet tileSet, out int size, out int gasType)
	{
		tileSet = type switch
		{
			PlanetType.Vortex => Vortex,
			PlanetType.Stardust => Stardust,
			PlanetType.Solar => Solar,
			_ => Nebula,
		};
		size = type switch
		{
			0 => 40,
			PlanetType.Stardust => 60,
			PlanetType.Solar => 80,
			_ => 55,
		};
		gasType = type switch
		{
			PlanetType.Nebula => ModContent.TileType<NebulaGas>(),
			PlanetType.Stardust => ModContent.TileType<Stardust>(),
			PlanetType.Solar => ModContent.TileType<SolarFlare>(),
			_ => ModContent.TileType<MiniVortex>(),
		};
	}

	private static void GeneratePlanetInstance(PlanetType type, FastNoiseLite noise, int x, int y, FastNoiseLite valueNoise, FastNoiseLite cellularNoise,
		PlanetTileSet tileSet, int size, int gasType, Dictionary<Point16, float> nebulaGasLocations)
	{
		bool hasSpikes = WorldGen.genRand.NextBool(2);
		int noiseAmp = WorldGen.genRand.Next(3, 11);
		int sizeWithBuffer = size + 20;

		if (hasSpikes)
		{
			noiseAmp += 6;
		}

		for (int i = x - sizeWithBuffer; i <= x + sizeWithBuffer; i++)
		{
			for (int j = y - sizeWithBuffer; j <= y + sizeWithBuffer; j++)
			{
				float cutoff = size - noise.GetNoise(i, j) * noiseAmp;
				float distance = Vector2.DistanceSquared(new Vector2(x, y), new Vector2(i, j));

				if (distance < cutoff * cutoff)
				{
					Tile tile = Main.tile[i, j];

					if (type == PlanetType.Nebula && distance > MathF.Pow(cutoff - 8, 2) && gasType != -1)
					{
						nebulaGasLocations.Add(new Point16(i, j), Utils.GetLerpValue(cutoff * cutoff, MathF.Pow(cutoff - 8, 2), distance));
						continue;
					}

					SetTilePerNoise(valueNoise, cellularNoise, tileSet, i, j);

					if (hasSpikes && WorldGen.genRand.NextBool(320))
					{
						var pos = new Vector2(i, j);
						var center = new Vector2(x, y);
						float length = size - pos.Distance(center) + WorldGen.genRand.Next(10, 30);

						if (length <= 10)
						{
							continue;
						}

						float angleRange = WorldGen.genRand.NextFloat(0.15f, 0.35f);
						bool isWall = WorldGen.genRand.NextBool(3);

						MoonlordTerrainGen.GenerateSpikeAction(new Point16(i, j), (int)length, pos.AngleFrom(center), angleRange,
							(x, y) => SetTilePerNoise(valueNoise, cellularNoise, tileSet, x, y, isWall), (_, _, _, _) => 1f);
					}
				}
			}
		}
	}

	private static void GenerateLuminiteSteps(int x, int y, int size, FastNoiseLite noise)
	{
		Point target = new(Main.maxTilesX / 2, MoonLordDomain.TopOfTheWorld);
		Point pos = new(x, y - size - 40);

		while (true)
		{
			pos += (pos.ToVector2().DirectionTo(target.ToVector2()) * WorldGen.genRand.Next(40, 90)).ToPoint();

			if (pos.ToVector2().DistanceSQ(target.ToVector2()) <= 80 * 80)
			{
				break;
			}

			PlaceLuminitePebble(noise, new Point(pos.X + WorldGen.genRand.Next(-20, 20), pos.Y + WorldGen.genRand.Next(-20, 20)), WorldGen.genRand.Next(12, 17));
		}

		PlaceLuminitePebble(noise, target, WorldGen.genRand.Next(20, 27));
	}

	private static void PlaceLuminitePebble(FastNoiseLite noise, Point pos, int size)
	{
		HashSet<Point16> positions = [];

		for (int i = pos.X - size; i <= pos.X + size; i++)
		{
			for (int j = pos.Y - size; j <= pos.Y + size; j++)
			{
				float distance = Vector2.DistanceSquared(pos.ToVector2(), new Vector2(i, j));
				float cutoff = size - noise.GetNoise(i, j) * 4;

				if (distance < cutoff * cutoff)
				{
					Tile tile = Main.tile[i, j];
					tile.HasTile = true;
					tile.TileType = !WorldGen.genRand.NextBool(8) ? TileID.LunarOre : WorldGen.genRand.Next(4) switch
					{
						0 => TileID.LunarBlockSolar,
						1 => TileID.LunarBlockNebula,
						2 => TileID.LunarBlockStardust,
						_ => TileID.LunarBlockVortex
					};

					positions.Add(new Point16(i, j));
				}
			}
		}

		foreach (Point16 position in positions)
		{
			Tile.SmoothSlope(position.X, position.Y);
		}
	}

	private static void GenerateNebulaGasRing(int gasType, Dictionary<Point16, float> nebulaGasLocations)
	{
		foreach (Point16 pos in nebulaGasLocations.Keys)
		{
			Tile tile = Main.tile[pos];

			if (tile.HasTile)
			{
				continue;
			}

			tile.HasTile = true;
			tile.TileType = (ushort)gasType;

			float value = nebulaGasLocations[pos];

			if (value < 0.33f)
			{
				tile.TileFrameX = 0;
			}
			else if (value < 0.67f)
			{
				tile.TileFrameX = 18;
			}
			else
			{
				tile.TileFrameX = 36;
			}
		}
	}

	private static void AddGasses(PlanetType type, int tile, Point16 position, int size)
	{
		if (tile == -1)
		{
			return;
		}

		if (type == PlanetType.Nebula)
		{
			for (int i = 0; i < 38; ++i)
			{
				Vector2 pos = position.ToVector2() + WorldGen.genRand.NextVector2CircularEdge(160, 100) * WorldGen.genRand.NextFloat(0.5f, 1f);
				pos = Vector2.Lerp(pos, position.ToVector2(), WorldGen.genRand.NextFloat(0, 0.5f));

				NebulaGasWisps(pos.ToPoint16(), tile, WorldGen.genRand.Next(15, 36));
			}
		}
		else if (type == PlanetType.Stardust)
		{
			for (int i = 0; i < 40; ++i)
			{
				Vector2 pos;

				do
				{
					pos = position.ToVector2() + WorldGen.genRand.NextVector2CircularEdge(160, 100) * WorldGen.genRand.NextFloat(0.5f, 1f);
					pos = Vector2.Lerp(pos, position.ToVector2(), WorldGen.genRand.NextFloat(0, 0.5f));
				} while (Main.tile[pos.ToPoint()].HasTile);

				StardustStarDusts(pos.ToPoint16(), tile, WorldGen.genRand.Next(1, 14));
			}
		}
		else if (type == PlanetType.Solar)
		{
			for (int i = 0; i < 70; ++i)
			{
				GenSolarBurst(tile, position, size);
			}
		}
		else
		{
			int direction = WorldGen.genRand.NextDirection();

			for (int i = 0; i < 20; ++i)
			{
				GenerateVortex(position, i / 20f * MathHelper.TwoPi, tile, WorldGen.genRand.NextFloat(0.02f, 0.06f) * direction, size * 6);
			}
		}
	}

	private static void GenerateVortex(Point16 position, float angle, int type, float angleMod, int size)
	{
		Vector2 dir = angle.ToRotationVector2();
		var pos = position.ToVector2();
		int length = WorldGen.genRand.Next(20) + size;

		for (int i = 0; i < length; ++i)
		{
			dir = dir.RotatedBy(angleMod);
			pos += dir;

			int width = (int)MathHelper.Lerp(12, 1, i / (float)length);

			for (int j = 0; j < width; ++j)
			{
				Vector2 widePos = pos + dir.RotatedBy(MathHelper.PiOver2) * j;
				Tile tile = Main.tile[widePos.ToPoint()];

				if (tile.HasTile)
				{
					continue;
				}

				tile.HasTile = true;
				tile.TileType = (ushort)type;
				tile.TileFrameX = (short)(WorldGen.genRand.Next(3) * 18);
				tile.TileFrameY = (short)(WorldGen.genRand.Next(3) * 18);
			}
		}
	}

	private static void GenSolarBurst(int type, Point16 position, int size)
	{
		Vector2 pos;

		do
		{
			pos = position.ToVector2() + WorldGen.genRand.NextVector2CircularEdge(160, 100) * WorldGen.genRand.NextFloat(0.5f, 1f);
			pos = Vector2.Lerp(pos, position.ToVector2(), WorldGen.genRand.NextFloat(0, 0.5f));
		} while (!WorldGen.SolidOrSlopedTile((int)pos.X, (int)pos.Y) || !Solar.ContainsTile(Main.tile[pos.ToPoint()].TileType));

		GenAction action = Actions.Chain(new Modifiers.Conditions(new NotSolid()), new Actions.PlaceTile((ushort)type));
		ShapeData data = new();
		int adjSize = Math.Max(size - 40, 0);
		GenShape shape = new ShapeRoot(pos.AngleFrom(position.ToVector2()), WorldGen.genRand.Next(20, 80) + adjSize, WorldGen.genRand.NextFloat(4, 8)).Output(data);
		WorldUtils.Gen(pos.ToPoint(), shape, action);

		foreach (Point16 point in data.GetData())
		{
			Point16 newPos = new(point.X + (int)pos.X, point.Y + (int)pos.Y);
			Tile tile = Main.tile[newPos];
			float distance = newPos.ToVector2().Distance(position.ToVector2());

			if (distance < size + 8)
			{
				tile.TileFrameX = 54;
			}
			else if (distance < size + 16)
			{
				tile.TileFrameX = 36;
			}
			else if (distance < size + 24)
			{
				tile.TileFrameX = 18;
			}
			else
			{
				tile.TileFrameX = 0;
			}
		}
	}

	private static void StardustStarDusts(Point16 pos, int type, int size, bool canRecurse = true)
	{
		for (int i = pos.X - size; i < pos.X + size + 1; ++i)
		{
			SetTile(i, pos.Y, Math.Abs(i - pos.X));
		}

		for (int j = pos.Y - size; j < pos.Y + size + 1; ++j)
		{
			SetTile(pos.X, j, Math.Abs(j - pos.Y));
		}

		if (size > 5 && WorldGen.genRand.NextBool(3) && canRecurse)
		{
			StardustStarDusts(new Point16(pos.X + WorldGen.genRand.NextDirection(), pos.Y + WorldGen.genRand.NextDirection()), type, size, false);
		}

		return;

		void SetTile(int x, int y, int distance)
		{
			Tile tile = Main.tile[x, y];
			
			if (tile.HasTile)
			{
				return;
			}

			tile.HasTile = true;
			tile.TileType = (ushort)type;

			if (distance < 2 && size > 5)
			{
				tile.TileFrameX = 36;
			}
			else if (distance < 4 && size > 2)
			{
				tile.TileFrameX = 18;
			}
			else
			{
				tile.TileFrameX = 0;
			}
		}
	}

	private static void NebulaGasWisps(Point16 pos, int type, int size)
	{
		for (int i = pos.X - size; i < pos.X + size; ++i)
		{
			Tile tile = Main.tile[i, pos.Y];

			if (tile.HasTile)
			{
				if (tile.TileType == type)
				{
					tile.TileFrameX = 36;
				}

				continue;
			}

			tile.HasTile = true;
			tile.TileType = (ushort)type;

			int distance = Math.Abs(i - pos.X);

			if (distance < size / 3f && size > 15)
			{
				tile.TileFrameX = 36;
			}
			else if (distance < size / 1.5f && size > 8)
			{
				tile.TileFrameX = 18;
			}
			else
			{
				tile.TileFrameX = 0;
			}
		}

		if (size > 10)
		{
			int quarterSize = size / 4;
			NebulaGasWisps(new Point16(pos.X, pos.Y - 1), type, size / 2 + WorldGen.genRand.Next(-quarterSize, quarterSize));
			NebulaGasWisps(new Point16(pos.X, pos.Y + 1), type, size / 2 + WorldGen.genRand.Next(-quarterSize, quarterSize));
		}
	}

	private static void SetTilePerNoise(FastNoiseLite valueNoise, FastNoiseLite cellularNoise, PlanetTileSet tileSet, int i, int j, bool wall = false)
	{
		float cellValue = cellularNoise.GetNoise(i, j);

		Tile tile = Main.tile[i, j];

		if (wall)
		{
			if (cellValue > -0.7f)
			{
				tile.WallType = (ushort)tileSet.WallMain;
			}
			else
			{
				tile.WallType = (ushort)(valueNoise.GetNoise(i, j) switch
				{
					< 0.1f => tileSet.WallMain,
					_ => tileSet.WallAlt
				});
			}
		}
		else
		{
			tile.HasTile = true;

			if (cellValue > -0.7f)
			{
				tile.TileType = (ushort)tileSet.SubAlt;
			}
			else
			{
				tile.TileType = (ushort)(valueNoise.GetNoise(i, j) switch
				{
					< -0.3f => tileSet.Main,
					< 0.1f => tileSet.Alt,
					< 0.5f => tileSet.Sub,
					_ => tileSet.SubAlt
				});
			}
		}
	}
}
