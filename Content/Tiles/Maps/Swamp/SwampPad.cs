using PathOfTerraria.Common.Tiles.FramingKinds;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.Maps.Swamp;

internal class SwampPad : ModTile, ILilyPadTile
{
	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileSolid[Type] = true;

		DustType = DustID.Grass;

		AddMapEntry(new Color(182, 175, 130));
	}
}
