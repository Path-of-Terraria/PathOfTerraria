using Microsoft.Xna.Framework.Graphics;
using PathOfTerraria.Common.Tiles;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mushroom;

internal class Mushpad : ModTile
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

	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
	{
		Tile tile = Main.tile[i, j];
		bool left = HasTile(i - 1, j);
		bool right = HasTile(i + 1, j);
		bool below = HasTile(i, j + 1);
		bool above = HasTile(i, j - 1);

		if (left && right)
		{
			if (below)
			{
				tile.TileFrameX = 54;
			}
			else
			{
				tile.TileFrameX = 18;
			}
		}
		else if (left && !right)
		{
			tile.TileFrameX = 36;
		}
		else if (!left && right)
		{
			tile.TileFrameX = 0;
		}
		else if (!left && !right)
		{
			if (above && below)
			{
				tile.TileFrameX = 72;
			}
			else if (above)
			{
				tile.TileFrameX = 90;
			}
			else
			{
				WorldGen.KillTile(i, j);
			}
		}
		else
		{
			WorldGen.KillTile(i, j);
		}

		tile.TileFrameY = (short)(Main.rand.Next(3) * 18);

		return false;

		static bool HasTile(int x, int y)
		{
			return Main.tile[x, y].HasTile && Main.tileSolid[Main.tile[x, y].TileType];
		}
	}
}
