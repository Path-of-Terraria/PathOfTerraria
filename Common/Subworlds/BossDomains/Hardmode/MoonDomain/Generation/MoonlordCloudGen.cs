using PathOfTerraria.Common.World.Generation;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain.Generation;

internal static class MoonlordCloudGen
{
	private delegate bool CloudDelegate(Point16 pos, out Rectangle bounds);

	internal static void GenerateClouds(GenerationProgress progress, int Width)
	{
		FastNoiseLite noise = new(WorldGen._genRandSeed);
		noise.SetFrequency(0.04f);
		noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);

		for (int i = 10; i < Width - 10; ++i)
		{
			int topHeight = (int)(noise.GetNoise(i * 0.15f, 0) * 40);
			int botHeight = (int)(noise.GetNoise(i * 0.3f, 500) * 40);

			int top = Math.Max(MoonLordDomain.TerrariaHeight - 320 - topHeight, MoonLordDomain.CloudTop);
			int bottom = Math.Min(MoonLordDomain.TerrariaHeight - 80 + botHeight, MoonLordDomain.CloudBottom);

			for (int j = top; j < bottom; ++j)
			{
				Tile tile = Main.tile[i, j];
				float heightFactor = Utils.GetLerpValue(top, bottom, j);
				float value = noise.GetNoise(i, j * 2f);
				value = FadeValue(top, bottom, j, value);

				if (value > MathHelper.Lerp(-0.1f, 0.2f, heightFactor))
				{
					tile.HasTile = true;
					tile.TileType = noise.GetNoise(i, j * 2.5f + 2000) > MathHelper.Lerp(-0.4f, 0.2f, heightFactor) ? TileID.RainCloud : TileID.Cloud;
				}

				if (noise.GetNoise(i, j + 1500) > 0.5f)
				{
					tile.TileType = TileID.SnowCloud;
				}

				value = noise.GetNoise(i + 10000, j * 2f);
				value = FadeValue(top, bottom, j, value);

				if (value > MathHelper.Lerp(-0.1f, 0.2f, heightFactor))
				{
					tile.WallType = WallID.Cloud;
				}
			}

			progress.Set(i / (Width - 20f));
		}

		SpamCloudObject(BuildPillar);
		SpamCloudObject(BuildGap);
		SpamCloudObject(BuildSmallStar, 18);
	}

	private static void SpamCloudObject(CloudDelegate cloud, int count = 12)
	{
		for (int i = 0; i < count; ++i)
		{
			Point16 pos;

			do
			{
				pos = new(WorldGen.genRand.Next(60, Main.maxTilesX - 60), WorldGen.genRand.Next(MoonLordDomain.CloudTop, MoonLordDomain.CloudBottom));
			} while (!GenVars.structures.CanPlace(new Rectangle(pos.X, pos.Y - 6, 12, 12)));

			bool success = cloud(pos, out Rectangle bounds);

			if (success)
			{
				GenVars.structures.AddProtectedStructure(bounds, 8);
			}
			else
			{
				i--;
			}
		}
	}

	private static bool BuildSmallStar(Point16 pos, out Rectangle bounds)
	{
		int size = WorldGen.genRand.Next(6, 12);
		bool wall = WorldGen.genRand.NextBool(4);

		ushort type;

		if (wall)
		{
			type = WorldGen.genRand.Next(2) switch
			{
				0 => WallID.Lavafall,
				_ => WallID.LavaMossBlockWall,
			};
		}
		else
		{
			type = WorldGen.genRand.Next(6) switch
			{
				0 => TileID.LivingFire,
				1 => TileID.LivingFrostFire,
				2 => TileID.LivingDemonFire,
				3 => TileID.Hellstone,
				4 => TileID.Meteorite,
				_ => TileID.LivingCursedFire,
			};
		}

		GenAction placeAction = wall ? new Actions.PlaceWall(type) : new Actions.PlaceTile(type);

		if (type == TileID.Meteorite)
		{
			placeAction = Actions.Chain(new Modifiers.Blotches(4, 0.4f), new Actions.PlaceTile(type));
		}

		WorldUtils.Gen(pos.ToPoint(), new Shapes.Circle(size), Actions.Chain(new Modifiers.Blotches(), new Actions.Clear()));
		int radius = (int)(size * WorldGen.genRand.NextFloat(0.4f, 0.7f));

		if (type == TileID.Meteorite)
		{
			radius = size + WorldGen.genRand.Next(2, 6);
		}
		
		WorldUtils.Gen(pos.ToPoint(), new Shapes.Circle(radius), placeAction);
		bounds = new Rectangle(pos.X - 10, pos.Y - 10, 20, 20);
		return true;
	}

	private static bool BuildPillar(Point16 pos, out Rectangle bounds)
	{
		bounds = Rectangle.Empty;

		if (WorldGen.SolidOrSlopedTile(pos.X, pos.Y))
		{
			return false;
		}

		HashSet<Point16> points = [];

		for (int i = pos.X - 4; i < pos.X + 4; ++i)
		{
			if (WorldGen.SolidOrSlopedTile(i, pos.Y))
			{
				continue;
			}

			if (!WorldUtils.Find(new Point(i, pos.Y), new Searches.Up(40).Conditions(new Conditions.IsSolid()), out Point top))
			{
				return false;
			}

			if (!WorldUtils.Find(new Point(i, pos.Y), new Searches.Down(40).Conditions(new Conditions.IsSolid()), out Point bottom))
			{
				return false;
			}

			for (int j = top.Y - 2; j < bottom.Y + 3; ++j)
			{
				points.Add(new Point16(i, j));
			}
		}

		Point topLeft = new(short.MaxValue - 1, short.MaxValue - 1);
		Point bottomRight = Point.Zero;

		foreach (Point16 point in points)
		{
			Tile tile = Main.tile[point.X, point.Y];

			if (tile.HasTile)
			{
				tile.TileType = TileID.Sunplate;
			}
			else
			{
				tile.WallType = WallID.DiscWall;
			}

			topLeft.X = Math.Min(topLeft.X, point.X);
			topLeft.Y = Math.Min(topLeft.Y, point.Y);

			bottomRight.X = Math.Max(bottomRight.X, point.X);
			bottomRight.Y = Math.Max(bottomRight.Y, point.Y);
		}

		bounds = new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
		return true;
	}

	private static bool BuildGap(Point16 pos, out Rectangle bounds)
	{
		bounds = Rectangle.Empty;

		if (WorldGen.SolidOrSlopedTile(pos.X, pos.Y))
		{
			return false;
		}

		HashSet<Point16> points = [];
		Point topLeft = new(short.MaxValue - 1, short.MaxValue - 1);
		Point bottomRight = Point.Zero;

		for (int j = pos.Y - 4; j < pos.Y + 4; ++j)
		{
			if (WorldGen.SolidOrSlopedTile(pos.X, j))
			{
				continue;
			}

			if (!WorldUtils.Find(new Point(pos.X, j), new Searches.Left(40).Conditions(new Conditions.IsSolid()), out Point left))
			{
				return false;
			}

			if (!WorldUtils.Find(new Point(pos.X, j), new Searches.Right(40).Conditions(new Conditions.IsSolid()), out Point right))
			{
				return false;
			}

			for (int i = left.X - 2; i < right.X + 3; ++i)
			{
				points.Add(new Point16(i, j));

				topLeft.X = Math.Min(topLeft.X, i);
				topLeft.Y = Math.Min(topLeft.Y, j);

				bottomRight.X = Math.Max(bottomRight.X, i);
				bottomRight.Y = Math.Max(bottomRight.Y, j);
			}
		}

		foreach (Point16 point in points)
		{
			Tile tile = Main.tile[point.X, point.Y];
			int wallDiff = point.Y - pos.Y;

			if (tile.HasTile)
			{
				tile.TileType = TileID.GoldStarryGlassBlock;
			}
			
			if (topLeft.Y < point.Y && bottomRight.Y > point.Y)
			{
				tile.WallType = WallID.GoldStarryGlassWall;
			}
		}

		bounds = new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
		return true;
	}

	private static float FadeValue(int top, int bottom, int j, float value)
	{
		if (j < top + 20)
		{
			value = MathHelper.Lerp(value, -0.4f, Math.Abs(j - (top + 20)) / 20f);
		}
		else if (j > bottom - 20)
		{
			value = MathHelper.Lerp(value, -0.4f, Math.Abs(j - (bottom - 20)) / 20f);
		}

		return value;
	}
}
