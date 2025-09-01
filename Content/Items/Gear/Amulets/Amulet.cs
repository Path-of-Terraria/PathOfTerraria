using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Amulets;

public abstract class Amulet : Gear
{
	protected override string GearLocalizationCategory => "Amulet";
	
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.DefaultToAccessory();

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Common.Enums.ItemType.Amulet;
	}
	
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 0.5f;
	}
	
	public override bool CanEquipAccessory(Player player, int slot, bool modded)
	{
		// Ensure amulets can be equipped in amulet slot only
		return slot == 5 || slot == 15;
	}
}