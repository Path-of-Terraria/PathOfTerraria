using System.Reflection;
using MonoMod.Cil;
using PathOfTerraria.Common.UI;

#nullable enable

namespace PathOfTerraria.Common.ModCompatibility;

internal class WikithisCompatibility : ModSystem
{
	private delegate bool PreDrawTooltipLineDetour(GlobalItem self, Item item, DrawableTooltipLine line, ref int yOffset);

	public override void Load()
	{
		if (!ModLoader.TryGetMod("Wikithis", out Mod wiki))
		{
			return;
		}

		const string WikithisItemType = "Wikithis.WikithisItem";

		if (wiki.Code.GetType(WikithisItemType) is Type itemType
		&& itemType.GetMethod(nameof(GlobalItem.PreDrawTooltipLine)) is MethodInfo preDrawTooltip)
		{
			try { MonoModHooks.Modify(preDrawTooltip, WsPreDrawTooltipLineInjection); }
			catch { } // TML will log this on its own.
		}
		else
		{
			Mod.Logger.Warn($"Could not inject into {WikithisItemType}.{nameof(GlobalItem.PreDrawTooltipLine)} method!");
		}

		// TODO: Our custom wiki is organized differently, in manner of /en/Gear/ItemName. This is not what Wikithis expects.
		/*
		if (!Main.dedServ)
		{
			try
			{
				// I'm pretty sure our custom wiki format doesn't work for Wikithis, but it's good to have I guess
				wiki.Call("AddModURL", PoTMod.Instance, "https://wiki.pathofterraria.com/{}");
			}
			catch { }
		}
		*/
	}

	private static void WsPreDrawTooltipLineInjection(ILContext ctx)
	{
		var il = new ILCursor(ctx);

		// Return true if StopDrawcode is true.
		ILLabel skipReturn = il.DefineLabel();
		il.EmitDelegate(() => Tooltip.SuppressDrawing);
		il.EmitBrfalse(skipReturn);
		il.EmitLdcI4(1);
		il.EmitRet();
		il.MarkLabel(skipReturn);
	}
}
