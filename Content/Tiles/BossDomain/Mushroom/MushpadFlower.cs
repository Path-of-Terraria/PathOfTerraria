using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain.Mushroom;

internal class MushpadFlower : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		TileID.Sets.BreakableWhenPlacing[Type] = false;
		TileID.Sets.PreventsSandfall[Type] = true;
		TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
		TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.Origin = new Point16(1, 1);
		TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<Mushpad>()];
		TileObjectData.addTile(Type);

		DustType = DustID.RedMoss;

		AddMapEntry(new Color(196, 74, 98));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		(r, g, b) = (0.5f, 0.05f, 0.05f);
	}
}