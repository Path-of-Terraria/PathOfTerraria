using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace PathOfTerraria.Common.UI.Armor;

public sealed class UIArmorInventoryHooks : ILoadable
{
	private static readonly MethodInfo DrawAccSlotsInfo = typeof(AccessorySlotLoader).GetMethod
	(
		nameof(AccessorySlotLoader.DrawAccSlots),
		BindingFlags.Public | BindingFlags.Instance
	);

	private static ILHook drawAccHook;

	void ILoadable.Load(Mod mod)
	{
		IL_Main.DrawInventory += DrawInventoryEdit;

		drawAccHook = new ILHook(DrawAccSlotsInfo, DrawAccSlotsEdit);
		drawAccHook.Apply();
	}

	void ILoadable.Unload()
	{
		drawAccHook?.Dispose();
		drawAccHook = null;
	}

	private static void DrawInventoryEdit(ILContext il)
	{
		var cursor = new ILCursor(il);

		ILLabel label = cursor.MarkLabel();

		// Match 'else if (EquipPage == 0)' for Release mode.
		if (!cursor.TryGotoNext
		(
			MoveType.After,
			i => i.MatchLdsfld<Main>(nameof(Main.EquipPage)),
			i => i.MatchBrtrue(out label)
		))
		{
			// Match it for Debug mode.
			cursor.GotoNext
			(
				MoveType.After,
				i => i.MatchLdsfld<Main>(nameof(Main.EquipPage)),
				i => i.MatchLdcI4(0),
				i => i.MatchCeq(),
				i => i.MatchStloc(out _),
				i => i.MatchLdloc(out _),
				i => i.MatchBrfalse(out label)
			);
		}

		cursor.EmitBr(label);
	}

	private static void DrawAccSlotsEdit(ILContext il)
	{
		var cursor = new ILCursor(il);

		cursor.Emit(OpCodes.Ret);
	}
}