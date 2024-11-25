using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using Terraria.Localization;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

internal class BurningRedBoots : Leggings, GenerateName.IItem
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 0.01f;
		staticData.IsUnique = true;
	}

	public override List<ItemAffix> GenerateImplicits()
	{
		return
		[
			(ItemAffix)Affix.CreateAffix<MovementSpeed>(),
			(ItemAffix)Affix.CreateAffix<MovementSpeed>(),
			(ItemAffix)Affix.CreateAffix<MovementSpeed>()
		];
	}

	string GenerateName.IItem.GenerateName(string defaultName)
	{
		return $"[c/FF0000:{Language.GetTextValue("Mods.PathOfTerraria.Items.BurningRedBoots.DisplayName")}]";
	}

	public override void PostRoll()
	{
		Item.defense = GetItemLevel.Invoke(Item) / 12;
	}
}