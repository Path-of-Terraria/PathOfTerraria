using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core;

namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Head)]
internal class Helmet : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Armor/Helmet/Base";

	protected override string GearLocalizationCategory => "Helmet";
	public override float DropChance => 1f;

	public override void Defaults()
	{
		ItemType = ItemType.Helmet;
	}

	public override void PostRoll()
	{
		Item.defense = ItemLevel / 10 + 1;
	}
}