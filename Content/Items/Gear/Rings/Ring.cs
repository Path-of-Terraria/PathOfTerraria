using PathOfTerraria.Core.Items;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Hooks;

namespace PathOfTerraria.Content.Items.Gear.Rings;

public abstract class Ring : Gear, IItemAllowDuplicateEquipWith
{
	protected override string GearLocalizationCategory => "Ring";
	
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.DefaultToAccessory();

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Common.Enums.ItemType.Ring;
	}
	
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 0.5f;
	}

	public override bool CanEquipAccessory(Player player, int slot, bool modded)
	{
		// Ensure rings can only be equipped in ring slots (7 and 8) (17 and 18 for vanity)
		return slot == 7 || slot == 8 || slot == 17 || slot == 18;
	}

	// Allow multiple rings of the same type to be equipped at once.
	bool IItemAllowDuplicateEquipWith.AllowDuplicateEquipWith(Item equippedItem, Item incomingItem, Player player)
	{
		if (equippedItem.ModItem is Ring && incomingItem.ModItem is Ring)
		{
			return true;
		}

		return false;
	}
}

