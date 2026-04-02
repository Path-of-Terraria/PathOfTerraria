using PathOfTerraria.Common.Tiles.FramingKinds;
using PathOfTerraria.Content.Swamp;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.Tiles;

internal class SwampPad : ModTile, ILilyPadTile
{
	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileSolid[Type] = true;

		TileID.Sets.BlocksWaterDrawingBehindSelf[Type] = false;

		DustType = DustID.Grass;

		AddMapEntry(new Color(159, 196, 125));
	}

	public override void NearbyEffects(int i, int j, bool closer)
	{
		if (SubworldSystem.Current is SwampArea)
		{
			Tile tile = Main.tile[i, j];
			tile.LiquidAmount = 150; // Water settling occasionally makes the lily pad look awful if not in water
		}
	}
}
