using Terraria.ID;
using Terraria.ObjectData;

namespace PathOfTerraria.Content.Tiles.Town;

internal class BrokenFurnace : ModTile
{
	public override void SetStaticDefaults()
	{
		Main.tileFrameImportant[Type] = true;

		TileID.Sets.InteractibleByNPCs[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
		TileObjectData.newTile.LavaDeath = false;
		TileObjectData.newTile.DrawYOffset = 2;
		TileObjectData.addTile(Type);

		AddMapEntry(new Color(109, 101, 89));
	}
}
