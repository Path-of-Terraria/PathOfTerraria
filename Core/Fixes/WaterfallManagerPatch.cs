using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Utilities;

#nullable enable

namespace PathOfTerraria.Core.Fixes;

/// <summary>
/// Fixes a difficult-to-reproduce vanilla index out of range crash caused by WaterfallManager.
/// </summary>
internal sealed class WaterfallManagerPatch : ModSystem
{
	public override void Load()
	{
		IL_WaterfallManager.FindWaterfalls += Injection;
	}

	private static void Injection(ILContext ctx)
	{
		bool result = InjectionInner(ctx);
		DebugUtils.DebugLog($"{nameof(WaterfallManagerPatch)}: {(result ? "Injection active" : "Injection failed")}.");
#if DEBUG && false
		MonoModHooks.DumpIL(PoTMod.Instance, ctx);
#endif
	}
	private static bool InjectionInner(ILContext ctx)
	{
		var il = new ILCursor(ctx);

		int x1Loc = -1;
		int x2Loc = -1;
		int y1Loc = -1;
		int y2Loc = -1;

		// First, find the 'y2 = Main.maxTilesY;' line, assumed to be last.
		if (!il.TryGotoNext(MoveType.Before, i => i.MatchLdsfld(typeof(Main), nameof(Main.maxTilesY)), i => i.MatchStloc(out y2Loc)) ) { return false; }
		// Then, going back, read all similar stores.
		if (!il.TryGotoPrev(MoveType.Before, i => i.MatchLdcI4(0), i => i.MatchStloc(out y1Loc)) ) { return false; }
		if (!il.TryGotoPrev(MoveType.Before, i => i.MatchLdsfld(typeof(Main), nameof(Main.maxTilesX)), i => i.MatchStloc(out x2Loc)) ) { return false; }
		if (!il.TryGotoPrev(MoveType.Before, i => i.MatchLdcI4(0), i => i.MatchStloc(out x1Loc)) ) { return false; }

		// Go to the start of the for loop, which starts as 'x = x1Loc'.
		if (!il.TryGotoNext(MoveType.Before, i => i.MatchLdloc(x1Loc), i => i.MatchStloc(out _)) ) { return false; }

		// Hijack labels, then modify the bounds.
		ILUtils.HijackIncomingLabels(il);
		il.Emit(OpCodes.Ldloca, x1Loc);
		il.Emit(OpCodes.Ldloca, x2Loc);
		il.Emit(OpCodes.Ldloca, y1Loc);
		il.Emit(OpCodes.Ldloca, y2Loc);
		il.EmitDelegate(FixupBounds);

		return true;
	}

	private static void FixupBounds(ref int x1, ref int x2, ref int y1, ref int y2)
	{
		x1 = Math.Max(x1, 1);
		y1 = Math.Max(y1, 1);
		x2 = Math.Min(x2, Main.maxTilesX - 1);
		y2 = Math.Min(y2, Main.maxTilesY - 1);
	}
}
