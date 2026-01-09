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

		/* 
		Needed art:
		* Cypress wood & leaves
		* Mangrove(?) wood & leaves. See reference: https://3.bp.blogspot.com/-e-_LY2o2eSA/WrjRW7GteUI/AAAAAAAAl0c/qwHZvwgqRTckU98e6ZwW9ebCQpiLECgTgCK4BGAYYCw/s1600/Florida%2Bmangroves%2B-%2BWikipedia-757401.jpg
		* 2x1 and 2x2, 3x2 swamp plants (cuttable ambient objects, like flowers, moss or grass) - for each size, 4-5 variants
		* 1x1, 2x1, 2x2 and 3x2 decor (mineable ambient objects, like mud, skulls or stone) - for each size, 4-5 variants
		* 1x1 and 2x1 Cypress and Mangrove root decor (found near the base of the respective trees, like the vanilla Living Wood decor tiles) - for each size, 4-5 variants
		* 1x1 and 2x1 Cypress and Mangrove wood decor (found on the wood of the respective tree) - for each size, 4-5 variants
		* 2x1 and 2x2 UNDERWATER swamp plants - for each size, 4-5 variants
		* Mangrove vine, thicker than normal vines to hold up platforms
		* Mangrove chest
		* Mangrove platform, either vine or wood
		* Mangrove spike tile
		* Three "thicknesses" of pond weed - think a vertical vine, one that's thin and leafless, then thicker and a few leaves, and then fully grown. Reply to this issue or ping me if you don't know how to sheet this
		* Two or three variants of lilypad flower
		* Standard tree variant for Swamp, go crazy with it lol
		*/
	}
}
