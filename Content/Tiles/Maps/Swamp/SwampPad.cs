using PathOfTerraria.Common.Subworlds.MappingAreas;
using PathOfTerraria.Common.Tiles.FramingKinds;
using SubworldLibrary;
using Terraria.ID;

namespace PathOfTerraria.Content.Tiles.Maps.Swamp;

internal class SwampPad : ModTile, ILilyPadTile
{
	public override void SetStaticDefaults()
	{
		Main.tileCut[Type] = true;
		Main.tileSolid[Type] = true;

		TileID.Sets.BlocksWaterDrawingBehindSelf[Type] = false;

		DustType = DustID.Grass;

		AddMapEntry(new Color(182, 175, 130));
	}

	public override void NearbyEffects(int i, int j, bool closer)
	{
		if (SubworldSystem.Current is SwampArea)
		{
			Tile tile = Main.tile[i, j];
			tile.LiquidAmount = 150;
		}
	}
}
