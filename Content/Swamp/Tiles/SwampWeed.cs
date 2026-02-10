using PathOfTerraria.Common.Tiles.FramingKinds;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.Tiles;

internal class SwampWeed : ModTile, IKelpTile
{
	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileSolid[Type] = false;
		Main.tileLighted[Type] = true;

		DustType = DustID.Grass;

		AddMapEntry(new Color(30, 81, 62));
	}

	public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
	{
		if (Main.tile[i, j].TileFrameNumber != 0)
		{
			(r, g, b) = (0.25f, 0.25f, 0.075f);
		}
	}
}
