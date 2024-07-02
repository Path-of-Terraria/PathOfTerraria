using PathOfTerraria.Core.Systems.Affixes;
using System.Collections.Generic;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class BurningRedBoots : Leggings
{
	public override float DropChance => 0.01f;
	public override bool IsUnique => true;

	public override List<ItemAffix> GenerateImplicits()
	{
		return new List<ItemAffix>() { (ItemAffix)Affix.CreateAffix<MovementSpeed>(0.8f) };
	}

	public override List<ItemAffix> GenerateAffixes()
	{
		return new List<ItemAffix>()
		{
			(ItemAffix)Affix.CreateAffix<MovementSpeed>(),
			(ItemAffix)Affix.CreateAffix<MovementSpeed>(),
			(ItemAffix)Affix.CreateAffix<MovementSpeed>()
		};
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