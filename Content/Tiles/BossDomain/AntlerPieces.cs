using PathOfTerraria.Content.Items.Quest;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class AntlerPieces : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileSolid[Type] = false;
		Main.tileFrameImportant[Type] = true;

		TileID.Sets.FramesOnKillWall[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.AnchorTop = AnchorData.Empty;
		TileObjectData.newTile.AnchorWall = true;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 6;
		TileObjectData.addTile(Type);

		DustType = DustID.WoodFurniture;

		AddMapEntry(new Color(122, 97, 85));
		RegisterItemDrop(ModContent.ItemType<AntlerShard>());
	}
}