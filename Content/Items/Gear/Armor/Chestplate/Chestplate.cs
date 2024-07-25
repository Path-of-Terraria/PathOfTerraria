using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core;

namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal class Chestplate : Gear
{
	public override string Texture => $"{nameof(PathOfTerraria)}/Assets/Items/Gear/Armor/Body/Base";

	protected override string GearLocalizationCategory => "Chestplate";
	public override float DropChance => 1f;

	public override void Defaults()
	{
		ItemType = ItemType.Chestplate;
	}

	public override void PostRoll()
	{
		Item.defense = ItemLevel / 6 + 1;
	}
}