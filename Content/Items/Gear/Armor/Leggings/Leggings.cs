using System.Collections.Generic;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class Leggings : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Armor/Legs/Base";

	protected override string GearLocalizationCategory => "Leggings";
	public override float DropChance => 1f;

	public override void Defaults()
	{
		ItemType = ItemType.Leggings;
	}

	public override List<ItemAffix> GenerateImplicits()
	{
		return [(ItemAffix)Affix.CreateAffix<MovementSpeed>(0.8f)];
	}

	public override void PostRoll()
	{
		Item.defense = ItemLevel / 12 + 1;
	}
}