using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.Town;

public class Ale : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileLavaDeath[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		
		TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.RandomStyleRange = 3;
		
		TileObjectData.addTile(Type);

		DustType = DustID.Glass;
		HitSound = SoundID.Shatter;
	}
	
	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
}