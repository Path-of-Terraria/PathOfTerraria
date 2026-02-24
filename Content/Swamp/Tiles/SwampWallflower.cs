using Terraria.ID;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.GameContent.Metadata;

namespace PathOfTerraria.Content.Swamp.Tiles;

internal class SwampWallflower : ModTile
{
    public override void SetStaticDefaults()
    {
		Main.tileFrameImportant[Type] = true;
		Main.tileCut[Type] = true;
		Main.tileSolid[Type] = false;
		Main.tileLighted[Type] = true;

		TileID.Sets.FramesOnKillWall[Type] = true;

		TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
		TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
		TileObjectData.newTile.AnchorWall = true;
        TileObjectData.newTile.RandomStyleRange = 3;
        TileObjectData.newTile.StyleHorizontal = true;
		TileObjectData.newTile.CoordinateWidth = 26;
		TileObjectData.newTile.CoordinateHeights = [26];
		TileObjectData.addTile(Type);

        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);
        TileID.Sets.SwaysInWindBasic[Type] = true;

		AddMapEntry(new Color(113, 142, 71));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		float mul = MathF.Sin(i + j + Main.GameUpdateCount * 0.02f) * 0.1f + 0.5f;
		(r, g, b) = (1f * mul, 0.85f * mul, 0.85f * mul);
	}

	public override void NumDust(int i, int j, bool fail, ref int num)
	{
		num = 3;
	}
}
