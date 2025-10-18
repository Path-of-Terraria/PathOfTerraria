using System.Reflection;
using MonoMod.Cil;

#nullable enable

namespace PathOfTerraria.Common.ModCompatibility;

internal class WikithisCompatibility : ModSystem
{
	private delegate bool PreDrawTooltipLineDetour(GlobalItem self, Item item, DrawableTooltipLine line, ref int yOffset);

	/// <summary>
	/// Stops Wikithis's global item from drawing when we don't want do. Automatically turned to on when the update loop is running.
	/// </summary>
	internal static bool StopDrawcode = false;

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

		On_Main.Update += JustStopDrawcode;
	
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

	private static void JustStopDrawcode(On_Main.orig_Update orig, Main self, GameTime gameTime)
	{
		StopDrawcode = true;
		orig(self, gameTime);
		StopDrawcode = false;
	}

	private static void WsPreDrawTooltipLineInjection(ILContext ctx)
	{
		var il = new ILCursor(ctx);

		// Return true if StopDrawcode is true.
		ILLabel skipReturn = il.DefineLabel();
		il.EmitDelegate(() => StopDrawcode);
		il.EmitBrfalse(skipReturn);
		il.EmitLdcI4(1);
		il.EmitRet();
		il.MarkLabel(skipReturn);
	}
}
