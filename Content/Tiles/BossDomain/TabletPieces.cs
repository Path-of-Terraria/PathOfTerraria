using PathOfTerraria.Content.Items.BossDomain;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class TabletPieces : ModTile
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
		TileObjectData.addTile(Type);

		DustType = DustID.AncientLight;

		AddMapEntry(new Color(147, 104, 87));

		RegisterItemDrop(ModContent.ItemType<AncientTabletOne>(), 0);
		RegisterItemDrop(ModContent.ItemType<AncientTabletTwo>(), 1);
		RegisterItemDrop(ModContent.ItemType<AncientTabletThree>(), 2);
		RegisterItemDrop(ModContent.ItemType<AncientTabletFour>(), 3);
	}
}
