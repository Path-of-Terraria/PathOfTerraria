using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class BurningRedBoots : Leggings
{
	public override float DropChance => 0.01f;
	public override bool IsUnique => true;

	public override List<ItemAffix> GenerateAffixes()
	{
		return
		[
			(ItemAffix)Affix.CreateAffix<MovementSpeed>(),
			(ItemAffix)Affix.CreateAffix<MovementSpeed>(),
			(ItemAffix)Affix.CreateAffix<MovementSpeed>()
		];
	}

	public override string GenerateName()
	{
		return $"[c/FF0000:{Language.GetTextValue("Mods.PathOfTerraria.Items.BurningRedBoots.DisplayName")}]";
	}

	public override void PostRoll()
	{
		Item.defense = ItemLevel / 12;
	}
}