using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using PathOfTerraria.Utilities;
using Terraria.ModLoader.Core;
using Terraria.UI;
using Hook = PathOfTerraria.Core.Hooks.IItemAllowDuplicateEquipWith;

namespace PathOfTerraria.Core.Hooks;

internal interface IItemAllowDuplicateEquipWith
{
	public static readonly GlobalHookList<GlobalItem> HookList = ItemLoader.AddModHook(GlobalHookList<GlobalItem>.Create(i => ((Hook)i).AllowDuplicateEquipWith));

	bool AllowDuplicateEquipWith(Item equippedItem, Item incomingItem, Player player);

	public static bool Invoke(Item equippedItem, Item incomingItem, Player player)
	{
		if (equippedItem.ModItem is Hook equippedModItem && equippedModItem.AllowDuplicateEquipWith(equippedItem, incomingItem, player))
		{
			return true;
		}

		if (incomingItem.ModItem is Hook incomingModItem && incomingModItem.AllowDuplicateEquipWith(equippedItem, incomingItem, player))
		{
			return true;
		}

		foreach (GlobalItem g in HookList.Enumerate(equippedItem))
		{
			if (((Hook)g).AllowDuplicateEquipWith(equippedItem, incomingItem, player))
			{
				return true;
			}
		}

		foreach (GlobalItem g in HookList.Enumerate(incomingItem))
		{
			if (((Hook)g).AllowDuplicateEquipWith(equippedItem, incomingItem, player))
			{
				return true;
			}
		}

		return false;
	}
}

file sealed class AllowDuplicateEquipWithHookImpl : ILoadable
{
	void ILoadable.Load(Mod mod)
	{
		// AccCheck_ForLocalPlayer was split with AccCheck_ForPlayer added in this commit:
		// https://github.com/tModLoader/tModLoader/commit/1a0c5e0
		// Can be simplified past ~2025-09-05.
		MethodInfo targetMethod = typeof(ItemSlot).GetMethod("AccCheck_ForPlayer", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
		MethodInfo fallbackMethod = typeof(ItemSlot).GetMethod("AccCheck_ForLocalPlayer", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
		MonoModHooks.Modify(targetMethod ?? fallbackMethod, AccCheckForPlayerInjection);
	}

	private static void AccCheckForPlayerInjection(ILContext ctx)
	{
		var il = new ILCursor(ctx);

		// Match 'if (item.IsTheSameAs(itemCollection[k]))'.
		ILLabel skipReturningTrue = null!;
		int locSlotIndex = -1;
		il.GotoNext(
			MoveType.Before,
			i => i.MatchLdarg2(),
			i => i.MatchLdarg1(),
			i => i.MatchLdloc(out locSlotIndex),
			i => i.MatchLdelemRef(),
			i => i.MatchCallOrCallvirt(typeof(Item).GetMethod("IsTheSameAs", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)),
			i => i.MatchBrfalse(out skipReturningTrue)
		);

		ILUtils.HijackIncomingLabels(il);

		il.Emit(OpCodes.Ldarg_1); // Equipped Item.
		il.Emit(OpCodes.Ldloc, locSlotIndex);
		il.Emit(OpCodes.Ldelem_Ref);
		il.Emit(OpCodes.Ldarg_2); // Incoming Item.
		il.Emit(OpCodes.Ldarg_0); // Player.
		il.EmitDelegate(Hook.Invoke);
		il.Emit(OpCodes.Brtrue, skipReturningTrue);
	}

	void ILoadable.Unload() { }
}
