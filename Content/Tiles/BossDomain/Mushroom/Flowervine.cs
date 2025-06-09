using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mushroom;

internal class Flowervine : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileBrick[Type] = true;
		Main.tileSolid[Type] = false;
		Main.tileBlockLight[Type] = true;
		Main.tileLighted[Type] = true;

		TileID.Sets.IsVine[Type] = true;
		TileID.Sets.VineThreads[Type] = true;
		TileID.Sets.ReplaceTileBreakDown[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.AlternateTile, 1, 0);
		TileObjectData.newTile.AnchorValidTiles = [Type, ModContent.TileType<Mushpad>(), TileID.MushroomGrass];
		TileObjectData.newTile.AnchorAlternateTiles = [Type, ModContent.TileType<Mushpad>(), TileID.MushroomGrass];
		TileObjectData.addTile(Type);

		DustType = DustID.RedMoss;

		AddMapEntry(new Color(196, 74, 98));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		(r, g, b) = (0.5f, 0.05f, 0.05f);
	}

	public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
	{
		Tile tile = Main.tile[i, j];
		bool below = HasTile(i, j + 1);
		bool above = HasTile(i, j - 1);

		if (!above)
		{
			WorldGen.KillTile(i, j);
			return false;
		}

		if (above && below)
		{
			tile.TileFrameX = 0;
		}
		else if (above)
		{
			tile.TileFrameX = 18;
		}

		tile.TileFrameY = (short)(Main.rand.Next(3) * 18);

		return false;

		bool HasTile(int x, int y)
		{
			return Main.tile[x, y].HasTile && (Main.tileSolid[Main.tile[x, y].TileType] || Main.tile[x, y].TileType == Type);
		}
	}
}
