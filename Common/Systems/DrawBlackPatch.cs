using SubworldLibrary;

namespace PathOfTerraria.Common.Systems;

internal sealed class DrawBlackPatch : ModSystem
{
	public override void Load()
	{
		base.Load();

		On_Main.DrawBlack += DrawBlackWithoutLayerOptimization;
	}

	private static void DrawBlackWithoutLayerOptimization(On_Main.orig_DrawBlack orig, Main self, bool force)
	{
		// We can specifically check for cases where this is known to occur,
		// such as the cave subworld, but checking for any subworld is fine for
		// now.
		// See: #211; DrawBlack attempts to optimize the range in which black
		// tiles are drawn by restricting the area based on the position
		// relative to half the world height, which does not account for
		// smaller-than-normal worlds.
		orig(self, force || SubworldSystem.AnyActive());
	}
}
