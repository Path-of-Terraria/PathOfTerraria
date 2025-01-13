using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;

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
			(ItemAffix)Affix.CreateAffix<MovementSpeedAffix>(),
			(ItemAffix)Affix.CreateAffix<MovementSpeedAffix>(),
			(ItemAffix)Affix.CreateAffix<MovementSpeedAffix>()
		];
	}

	string GenerateName.IItem.GenerateName(string defaultName)
	{
		return Language.GetTextValue("Mods.PathOfTerraria.Items.BurningRedBoots.DisplayName");
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		TooltipLine nameTip = tooltips.First(x => x.Name == "ItemName");

		if (nameTip is not null)
		{
			nameTip.OverrideColor = Color.Red;
		}
	}

	public override void PostRoll()
	{
		Item.defense = GetItemLevel.Invoke(Item) / 12;
	}
}