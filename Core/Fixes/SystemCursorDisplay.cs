#if DEBUG
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace PathOfTerraria.Core.Fixes;

/// <summary>
/// A simple tweak that makes the Operational System's cursor be displayed whenever UI is toggled off.
/// Useful for recording footage with interface hidden.
/// </summary>
internal sealed class SystemCursorDisplay : ILoadable
{
	void ILoadable.Load(Mod mod)
	{
		Main.QueueMainThreadAction(() => IL_Main.DoUpdate += Injection);
	}

	void ILoadable.Unload() { }

	private static void Injection(ILContext context)
	{
		var il = new ILCursor(context);

		// Match 'IsMouseVisible = false;'.
		if (!il.TryGotoNext(
			MoveType.Before,
			i => i.MatchLdarg(0),
			i => i.MatchLdcI4(0),
			i => i.MatchCall(typeof(Game), "set_IsMouseVisible")
		)) {
			// Not that important.
			PoTMod.Instance.Logger.Warn($"{nameof(SystemCursorDisplay)}'s injection failed.");
			return;
		}

		il.Index += 2;

		il.Emit(OpCodes.Pop); // Pop the zero
		il.EmitDelegate(ShouldDisplayMouseCursor);
	}

	private static bool ShouldDisplayMouseCursor()
	{
		return Main.hideUI && !Main.gameMenu;
	}
}
#endif
