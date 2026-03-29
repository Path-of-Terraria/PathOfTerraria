using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.Town;

public class HiddenBookcase : ModTile
{
	public static bool HoveringOverBook(int i, int j)
	{
		Tile tile = Main.tile[i, j];
		return tile.TileFrameX == 36 && tile.TileFrameY is 18 or 36;
	}

	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileLavaDeath[Type] = true;
		Main.tileNoAttach[Type] = true;
		Main.tileSolidTop[Type] = true;

		TileID.Sets.IsATrigger[Type] = true;
		
		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
		TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 18];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 1;
		TileObjectData.newTile.Origin = new Point16(1, 2);
		TileObjectData.newTile.DrawYOffset = 2;
		TileObjectData.addTile(Type);

		DustType = DustID.WoodFurniture;
		HitSound = SoundID.Dig;
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}

	public override bool RightClick(int i, int j)
	{
		if (HoveringOverBook(i, j))
		{
			Wiring.TripWire(i, j, 1, 1);
			return true;
		}

		return false;
	}

	public override void MouseOver(int i, int j)
	{
		if (!HoveringOverBook(i, j))
		{
			return;
		}

		Player player = Main.LocalPlayer;
		player.cursorItemIconID = ItemID.Book;
		player.noThrow = 2;
		player.cursorItemIconEnabled = true;
	}
}