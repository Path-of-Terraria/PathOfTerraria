using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Rings;

public abstract class Ring : Gear
{
	protected override string GearLocalizationCategory => "Ring";
	
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.DefaultToAccessory();

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Common.Enums.ItemType.Ring;
	}
}