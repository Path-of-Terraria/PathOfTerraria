using PathOfTerraria.Content.GUI;
using PathOfTerraria.Core.Loaders.UILoading;
using System.Linq;
using Terraria.UI;

namespace PathOfTerraria.Core.Systems;

internal class HotbarHijack : ModSystem
{
	public override void Load()
	{
		On_ItemSlot.LeftClick_ItemArray_int_int += StopHotbar;
		On_Main.GUIHotbarDrawInner += StopHoverText;
	}

	private void StopHoverText(On_Main.orig_GUIHotbarDrawInner orig, Main self)
	{
		string lastHover = Main.hoverItemName;

		orig(self);

		if (Main.LocalPlayer.selectedItem == 0 && !Main.hoverItemName.StartsWith(Main.LocalPlayer.HeldItem.AffixName()))
		{
			Main.hoverItemName = lastHover;
		}
	}

	private void StopHotbar(On_ItemSlot.orig_LeftClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
	{
		if (context == ItemSlot.Context.InventoryItem && !Main.mouseItem.IsAir && slot <= 9)
		{
			Item item = Main.mouseItem;

			bool weapon = item.damage > 0 && item.pick <= 0;
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