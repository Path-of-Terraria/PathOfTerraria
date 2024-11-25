using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Offhands;

public abstract class Offhand : Gear
{
	protected override string GearLocalizationCategory => "Offhand";
	
	public override void SetDefaults()
	{
		base.SetDefaults();

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Common.Enums.ItemType.Offhand;
	}
}