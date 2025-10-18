using MonoMod.RuntimeDetour;

#nullable enable

namespace PathOfTerraria.Common.ModCompatibility;

internal class WikithisCompatibility : ModSystem
{
	private delegate bool PreDrawTooltipLineDetour(GlobalItem self, Item item, DrawableTooltipLine line, ref int yOffset);

	/// <summary>
	/// Stops Wikithis's global item from drawing when we don't want do. Automatically turned to on when the update loop is running.
	/// </summary>
	internal static bool StopDrawcode = false;

	private static Hook? WikithisItemPreDrawTooltipHook = null;

	public override void Load()
	{
		if (!ModLoader.TryGetMod("Wikithis", out Mod wiki))
		{
			return;
		}

		Type itemType = wiki.GetType().Assembly.GetType("Wikithis.WikithisItem");
		WikithisItemPreDrawTooltipHook = new Hook(itemType.GetMethod(nameof(GlobalItem.PreDrawTooltipLine)), DetourWikithisPreDrawTooltipLine);

		On_Main.Update += JustStopDrawcode;
	
		if (wiki != null && !Main.dedServ)
		{
			// I'm pretty sure our custom wiki format doesn't work for Wikithis, but it's good to have I guess
			wiki.Call("AddModURL", this, "https://wiki.pathofterraria.com/{}");
		}
	}

	private void JustStopDrawcode(On_Main.orig_Update orig, Main self, GameTime gameTime)
	{
		StopDrawcode = true;
		orig(self, gameTime);
		StopDrawcode = false;
	}

	private static bool DetourWikithisPreDrawTooltipLine(PreDrawTooltipLineDetour orig, GlobalItem self, Item item, DrawableTooltipLine line, ref int yOffset)
	{
		if (StopDrawcode)
		{
			return true;
		}

		return orig(self, item, line, ref yOffset);
	}
}
