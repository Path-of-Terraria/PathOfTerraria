using PathOfTerraria.Core.Systems.Affixes;
using System.Collections.Generic;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes;
using Terraria.Localization;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class BurningRedBoots : Leggings, IGenerateImplicitsItem, GenerateName.IItem
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 0.01f;
		staticData.IsUnique = true;
	}

	public override List<ItemAffix> GenerateAffixes(Item item)
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

	public override void PostRoll(Item item)
	{
		Item.defense = GetItemLevel.Invoke(item) / 12;
	}
}