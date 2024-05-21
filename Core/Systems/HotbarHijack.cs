using Terraria.UI;

namespace PathOfTerraria.Core.Systems
{
	internal class HotbarHijack : ModSystem
	{
		public override void Load()
		{
			On_ItemSlot.LeftClick_ItemArray_int_int += StopHotbar;
		}

		private void StopHotbar(On_ItemSlot.orig_LeftClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
		{
			if (context == 0 && !Main.mouseItem.IsAir && slot <= 9)
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
				if (slot < 2) return;
				if (weapon || tool) return;
				orig(inv, context, slot);
				return;
			}

			orig(inv, context, slot);
		}
	}
}
