using PathOfTerraria.Common.World.Generation;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace PathOfTerraria.Common.Subworlds.BossDomains.Hardmode.MoonDomain.Generation;

internal static class MoonlordCloudGen
{
	internal static void GenerateClouds(GenerationProgress progress, int Width, int Height)
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

				value = noise.GetNoise(i + 10000, j * 2f);
				value = FadeValue(top, bottom, j, value);

				if (value > MathHelper.Lerp(-0.1f, 0.2f, heightFactor))
				{
					tile.WallType = WallID.Cloud;
				}
			}

			progress.Set(i / (Width - 20f));
		}

		for (int i = 0; i < 20; ++i)
		{
			Point16 pos;

			do
			{
				pos = new(WorldGen.genRand.Next(60, Width - 60), WorldGen.genRand.Next(MoonLordDomain.CloudTop, MoonLordDomain.CloudBottom));
			} while (!GenVars.structures.CanPlace(new Rectangle(pos.X, pos.Y - 6, 12, 12)));

			if (BuildPillar(pos, out Rectangle bounds))
			{
				GenVars.structures.AddProtectedStructure(bounds, 8);
			}
			else
			{
				i--;
			}
		}
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
