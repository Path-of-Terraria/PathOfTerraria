using PathOfTerraria.Common.Subworlds.BossDomains.Hardmode;
using PathOfTerraria.Common.Tiles.FramingKinds;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mushroom;

internal class Mushpad : ModTile, ILilyPadTile
{
	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileLighted[Type] = true;
		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = true;

		DustType = DustID.GlowingMushroom;

		AddMapEntry(new Color(182, 175, 130));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		Tile tile = Main.tile[i, j];
		float str = Main.rand.Next(28, 42) * 0.005f;
		str += (270 - Main.mouseTextColor) / 1000f;

		if (tile.TileColor == PaintID.None)
		{
			r = 0f;
			g = 0.2f + str / 2f;
			b = 1f;
		}
		else
		{
			Color color2 = WorldGen.paintColor(tile.TileColor);
			r = color2.R / 255f;
			g = color2.G / 255f;
			b = color2.B / 255f;
		}
	}

	void ILilyPadTile.PlacePad(int x, int y, bool overRide)
	{
		int width = WorldGen.genRand.Next(9, 21);

		for (int i = x - width + 1; i < x + width; ++i)
		{
			Tile tile = Main.tile[i, y];

			if (tile.HasTile && !overRide)
			{
				continue;
			}

			tile.HasTile = true;
			tile.TileType = Type;

			if (i == x)
			{
				for (int j = 1; j < width / 3; ++j)
				{
					Tile stem = Main.tile[i, y + j];
					stem.HasTile = true;
					stem.TileType = Type;
				}
			}
		}

		for (int i = x - width; i < x + width; ++i)
		{
			WorldGen.TileFrame(i, y);

			if (i == x)
			{
				for (int j = 1; j < width / 3; ++j)
				{
					WorldGen.TileFrame(i, y + j);
				}
			}
			else
			{
				FishronDomain.SpawnMushroomVine(i, y);
			}
		}
	}
}
