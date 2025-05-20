using PathOfTerraria.Common.Systems.Networking.Handlers;
using PathOfTerraria.Common.Tiles;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.BossDomain;

internal class GlowingSpores : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLighted[Type] = true;

		TileID.Sets.FramesOnKillWall[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.RandomStyleRange = 3;
		TileObjectData.newTile.AnchorWall = true;
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.addTile(Type);

		DustType = DustID.CorruptGibs;

		AddMapEntry(new Color(102, 115, 15));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		Tile tile = Main.tile[i, j];
		int frameX = tile.TileFrameX / 18 % 3;

		(r, g, b) = frameX switch
		{
			0 => (0.3f, 0.5f, 0.2f),
			1 => (0.1f, 0.6f, 0.15f),
			_ => (0.3f, 0.6f, 0.25f)
		};
	}
}