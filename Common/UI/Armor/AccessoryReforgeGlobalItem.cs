using PathOfTerraria.Content.Items.Gear.Amulets;
using PathOfTerraria.Content.Items.Gear.Offhands;
using PathOfTerraria.Content.Items.Gear.Rings;

namespace PathOfTerraria.Common.UI.Armor;

public sealed class AccessoryReforgeGlobalItem : GlobalItem
{
	public override void Load()
	{
		base.Load();
		
		On_Item.CanHavePrefixes += Item_CanHavePrefixes_Hook;
	}

	public override bool CanReforge(Item item)
	{
		return !item.accessory;
	}
	
	public override void UpdateInventory(Item item, Player player)
	{
		base.UpdateInventory(item, player);

		if (!item.accessory || item.prefix <= 0)
		{
			return;
		}

		item.ResetPrefix();
	}
	
	private static bool Item_CanHavePrefixes_Hook(On_Item.orig_CanHavePrefixes orig, Item self)
	{
		return self.accessory ? false : orig(self);
	}
}