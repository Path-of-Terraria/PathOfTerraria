using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.Furniture;

public class DeerTrophy : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;
		Main.tileBlockLight[Type] = true;
		Main.tileLavaDeath[Type] = true;
		Main.tileNoAttach[Type] = true;

		TileID.Sets.FramesOnKillWall[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(120, 85, 60), Language.GetText("MapObject.Trophy"));
		
		DustType = DustID.WoodFurniture;
		HitSound = SoundID.Dig;
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = fail ? 1 : 3;
	}
}