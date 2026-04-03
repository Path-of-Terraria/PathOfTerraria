using Terraria.ID;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;

namespace PathOfTerraria.Content.Swamp.Tiles;

internal class SwampPlants1x1 : ModTile
{
    public override void SetStaticDefaults()
    {
		Main.tileFrameImportant[Type] = true;
		Main.tileCut[Type] = true;
		Main.tileSolid[Type] = false;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
        TileObjectData.newTile.AnchorValidTiles = [ModContent.TileType<SwampGrass>()];
        TileObjectData.newTile.RandomStyleRange = 6;
        TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.addTile(Type);

        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);
        TileID.Sets.SwaysInWindBasic[Type] = true;
    }

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = 3;
	}

	public override void SetSpriteEffects(int i, int j, ref SpriteEffects effects)
	{
		effects = i % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
	}
}
