using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Systems.RealtimeGen;
using PathOfTerraria.Common.World.Generation;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mushroom;

internal class Burstshroom2x2 : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.Origin = new Point16(0, 1);
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 4;
		TileObjectData.addTile(Type);

		DustType = DustID.GlowingMushroom;

		AddMapEntry(new Color(95, 98, 215));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		Tile tile = Main.tile[i, j];
		float str = Main.rand.Next(28, 42) * 0.005f;
		str += (270 - Main.mouseTextColor) / 1000f;

		if (tile.TileColor == PaintID.None)
		{
			r = 0.1f;
			g = 0.8f + str / 2f;
			b = 3.2f;
		}
		else
		{
			Color color2 = WorldGen.paintColor(tile.TileColor);
			r = color2.R / 255f;
			g = color2.G / 255f;
			b = color2.B / 255f;
		}
	}

	public override void KillMultiTile(int i, int j, int frameX, int frameY)
	{
		WindingCavern(i, j + 4, Main.rand.Next(45, 60));

		FishronDomain.MushroomsBroken++;
	}

	public static void WindingCavern(int x, int y, int depth)
	{
		PriorityQueue<RealtimeStep, float> steps = new();

		FastNoiseLite diggingNoise = new(WorldGen._genRandSeed);
		diggingNoise.SetFrequency(0.1f);

		FastNoiseLite windingNoise = new(WorldGen._genRandSeed);
		windingNoise.SetFrequency(0.03f);

		Point16 highPos = new(0, 1200);
		int order = 0;

		for (int j = y; j > y - depth; j--)
		{
			int useX = x - (int)(windingNoise.GetNoise(x, j * 0.7f) * 6);
			int minDistance = (int)MathHelper.Lerp(8, 2, (y - j) / (float)depth);

			for (int i = useX - minDistance; i < useX + minDistance; ++i)
			{
				Tile tile = Main.tile[i, j];

				steps.Enqueue(RealtimeSteps.KillTile(i, j, true), order++);
				steps.Enqueue(RealtimeSteps.PlaceTile(i, j, TileID.MushroomBlock), order++);

				if (j < highPos.Y)
				{
					highPos = new Point16(i, j);
				}
			}
		}

		int HalfTopWidth = Main.rand.Next(29, 37);

		highPos = new Point16(highPos.X, highPos.Y - 4);

		for (int i = highPos.X - HalfTopWidth; i < highPos.X + HalfTopWidth; ++i)
		{
			for (int j = highPos.Y - HalfTopWidth; j < highPos.Y + HalfTopWidth; ++j)
			{
				float useJ = j;

				if (j > highPos.Y)
				{
					useJ = MathHelper.Lerp(useJ, highPos.Y + (HalfTopWidth - 4), WorldGen.genRand.NextFloat(0.5f, 0.6f));
				}

				float dist = Vector2.Distance(highPos.ToVector2(), new Vector2(i, useJ));

				if (dist < HalfTopWidth * 0.6f - WorldGen.genRand.NextFloat())
				{
					int off = highPos.Y - j;
					steps.Enqueue(RealtimeSteps.KillTile(i, j, true), order + HalfTopWidth + off * 2);
					steps.Enqueue(RealtimeSteps.PlaceTile(i, j, TileID.MushroomBlock), order + HalfTopWidth + off * 2 + 3);
				}
			}
		}

		List<RealtimeStep> useSteps = [];

		while (steps.Count > 0)
		{
			useSteps.Add(steps.Dequeue());
		}

		RealtimeGenerationSystem.AddAction(new RealtimeGenerationAction(useSteps, 0.004f));
	}
}
