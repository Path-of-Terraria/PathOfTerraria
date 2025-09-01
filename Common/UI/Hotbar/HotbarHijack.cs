using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Hotbar;

internal sealed class HotbarHijack : ModSystem
{
	public override void Load()
	{
		IL_Main.GUIHotbarDrawInner += StopVanillaHotbarDrawing;
		On_ItemSlot.LeftClick_ItemArray_int_int += ReserveHotbarSlots_PreventLeftClickingItemsIntoHotbar;
		On_Player.GetItem_FillEmptyInventorySlot += ReserveHotbarSlors_PreventFillingHotbarWithItems;
	}

	private static void StopVanillaHotbarDrawing(ILContext ctx)
	{
		// Simply short-circuit all hotbar rendering, while still letting other mods' detour into this function run as usual.
		var il = new ILCursor(ctx);
		il.Emit(OpCodes.Ret);
	}

	private static void ReserveHotbarSlots_PreventLeftClickingItemsIntoHotbar(On_ItemSlot.orig_LeftClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
	{
		if (context != ItemSlot.Context.InventoryItem || Main.mouseItem.IsAir || slot > 9)
		{
			orig(inv, context, slot);
			return;
		}

		Item item = Main.mouseItem;
		bool weapon = IsWeapon(item);
		bool tool = IsTool(item);

		// Slot 1 should be forced to be the equipped weapon
		if (weapon && slot == 0)
		{
			orig(inv, context, slot);
			return;
		}

		// Slot 2 should be forced to be the equipped tool
		if (tool && slot == 1)
		{
			orig(inv, context, slot);
			return;
		}

		if (slot < 2)
		{
			return;
		}

		// Tools can go elsewhere, weapons can't
		if (weapon && !tool)
		{
			return;
		}

		orig(inv, context, slot);
	}

	private static bool ReserveHotbarSlors_PreventFillingHotbarWithItems(On_Player.orig_GetItem_FillEmptyInventorySlot orig, Player self, int plr, Item newItem, 
		GetItemSettings settings, Item returnItem, int i)
	{
		// If not part of the hotbar, we don't care.
		if (i > 9)
		{
			return orig(self, plr, newItem, settings, returnItem, i);
		}

		bool weapon = IsWeapon(newItem);
		bool tool = IsTool(newItem);

		// Allow filling slot 1 with a weapon if there is no item.
		if (weapon && i == 0)
		{
			return orig(self, plr, newItem, settings, returnItem, i);
		}

		// Allow filling slot 2 with a tool if there is no item.
		if (tool && i == 1)
		{
			return orig(self, plr, newItem, settings, returnItem, i);
		}

		if (i < 2)
		{
			return false;
		}

		// Tools can go elsewhere, weapons can't.
		if (weapon && !tool)
		{
			return false;
		}

		return orig(self, plr, newItem, settings, returnItem, i);
	}

	public static bool IsWeapon(Item item)
	{
		return item.damage > 0 && item.ammo == AmmoID.None;
	}

	private static bool IsTool(Item item)
	{
		return item.pick > 0 || item.axe > 0 || item.hammer > 0;
	}
}

public class WeaponPickupItem : GlobalItem
{
	public override bool CanPickup(Item item, Player player)
	{
		if (player.IsVoidVaultEnabled)
		{
			// If the player has the Void Vault open & there's space, allow pickup
			return true;
		}

		if (HotbarHijack.IsWeapon(item))
		{
			if (player.inventory[0].IsAir)
			{
				return true;
			}

			for (int i = 10; i < Main.InventoryItemSlotsCount; i++)
			{
				Item inv = player.inventory[i];

				if (inv.IsAir)
				{
					return true;
				}
			}

			return false;
		}

		return true;
	}
}