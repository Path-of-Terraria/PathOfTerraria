using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Head)]
internal class Helmet : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Armor/Helmet/Base";

	protected override string GearLocalizationCategory => "Helmet";
	public override float DropChance => 1f;

	public override void SetDefaults()
	{
		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Helmet;
	}

	public override void PostRoll(Item item)
	{
		Item.defense = ItemLevel / 10 + 1;
	}
}