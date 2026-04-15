using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp;

internal class BloatingLeech : Gear
{
	protected override string GearLocalizationCategory => "Summon";

	public override void SetDefaults()
	{
		Item.DamageType = DamageClass.Summon;
		Item.damage = 150;
		Item.useTime = 20;
		Item.useAnimation = 20;
		Item.channel = true;
		Item.useStyle = ItemUseStyleID.Swing;
		//Item.shoeSlot = 

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Common.Enums.ItemType.Summon;
	}
}
