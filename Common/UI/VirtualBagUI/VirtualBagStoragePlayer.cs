using PathOfTerraria.Common.Config;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.UI;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.VirtualBagUI;

internal class VirtualBagStoragePlayer : ModPlayer 
{
	public static bool LocalUseVirtualBag => Main.LocalPlayer.GetModPlayer<VirtualBagStoragePlayer>().UsesVirtualBag;

	public List<Item> Storage = [];
	public bool UsesVirtualBag = true;

	public override void Load()
	{
		On_ItemSlot.RightClick_ItemArray_int_int += OverrideRightClickForVirtualBag;
	}

	public override void OnEnterWorld()
	{
		if (Main.myPlayer == Player.whoAmI)
		{
			UsesVirtualBag = ModContent.GetInstance<UIConfig>().UseVirtualBag;
		}
	}

	private void OverrideRightClickForVirtualBag(On_ItemSlot.orig_RightClick_ItemArray_int_int orig, Item[] inv, int context, int slot)
	{
		bool clicked = Main.mouseRight && Main.mouseRightRelease;

		if (UIManager.TryGet(VirtualBagUIState.Identifier, out UIManager.UIStateData data) && data.Enabled && VirtualBagAllowed(inv[slot], Main.LocalPlayer) && clicked)
		{
			Main.LocalPlayer.GetModPlayer<VirtualBagStoragePlayer>().Storage.Add(inv[slot].Clone());
			(data.Value as VirtualBagUIState).RefreshStorage();
			inv[slot].TurnToAir();

			SoundEngine.PlaySound(SoundID.Grab);
			return;
		}

		orig(inv, context, slot);
	}

	public static bool VirtualBagAllowed(Item item, Player player)
	{
		return (!item.IsACoin && (item.ModItem is Gear || item.TryGetGlobalItem(out PoTGlobalItem _))) && player.GetModPlayer<VirtualBagStoragePlayer>().UsesVirtualBag;
	}

	public override void Unload()
	{
		UIManager.TryDisable(VirtualBagUIState.Identifier);
	}
}
