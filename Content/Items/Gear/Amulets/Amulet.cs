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
}