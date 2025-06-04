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
		TileObjectData.newTile.Origin = new Point16(2, 1);
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
}
