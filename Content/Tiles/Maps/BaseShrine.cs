using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.Maps;

public abstract class BaseShrine : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileNoAttach[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
		TileObjectData.newTile.CoordinateHeights = [16, 18];
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.StyleWrapLimit = 3;
		TileObjectData.newTile.Origin = new Point16(1, 0);
		TileObjectData.addTile(Type);

		DustType = DustID.Stone;
		HitSound = SoundID.Dig;
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
}