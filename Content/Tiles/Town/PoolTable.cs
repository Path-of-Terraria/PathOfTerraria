using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.Town;

public class PoolTable : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
		TileObjectData.newTile.Width = 4;
		TileObjectData.addTile(Type);

		DustType = DustID.WoodFurniture;
		HitSound = SoundID.Dig;
	}
	
	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
}