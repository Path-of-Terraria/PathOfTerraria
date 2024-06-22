using PathOfTerraria.Content.GUI;
using PathOfTerraria.Core.Loaders.UILoading;
using System.Linq;
using PathOfTerraria.Helpers;
using Terraria.UI;

namespace PathOfTerraria.Core.Systems;

internal class HotbarHijack : ModSystem
{
	public override void Load()
	{
		On_ItemSlot.LeftClick_ItemArray_int_int += StopHotbar;
		On_Main.GUIHotbarDrawInner += StopVanillaHotbarDrawing;
	}

	private void StopVanillaHotbarDrawing(On_Main.orig_GUIHotbarDrawInner orig, Main self)
	{
		// This detour previously handled preventing the hover text that would
		// draw when hovering over items in the hotbar.
		// It has been reworked to prevent all associated drawing of the vanilla
		// hotbar instead.
		// This allows for other mods' detours to run if they hook this method
		// while still sufficiently preventing the vanilla hotbar from drawing.

		// Always set it to true to cause an early return in the vanilla method.
		bool origPlayerInventory = Main.playerInventory;
		Main.playerInventory = true;

		orig(self);

		Main.playerInventory = origPlayerInventory;
	}

	private void StopHotbar(On_ItemSlot.orig_LeftClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
	{
		if (context == ItemSlot.Context.InventoryItem && !Main.mouseItem.IsAir && slot <= 9)
		{
			Item item = Main.mouseItem;

			bool weapon = item.IsWeapon();
			bool tool = item.pick > 0;

			// Slot 1 should be forced to be the equipped weapon
			if (weapon && slot == 0)
			{
				orig(inv, context, slot);
				return;
			}

			// Slot 2 should be forced to be the equipped pickaxe
			if (tool && slot == 1)
			{
				orig(inv, context, slot);
				return;
			}

			// The rest of the slots cannot be weapons or picks
			if (slot < 2)
			{
				return;
			}

			if (weapon || tool)
			{
				return;
			}

			orig(inv, context, slot);
			return;
		}

		orig(inv, context, slot);
	}
}