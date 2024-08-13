using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Enums;

namespace PathOfTerraria.Content.Items.Gear;

public abstract class Gear : ModItem, GenerateAffixes.IItem, GenerateImplicits.IItem, PostRoll.IItem, GearLocalizationCategory.IItem
{
	protected virtual string GearLocalizationCategory => GetType().Name;

	string GearLocalizationCategory.IItem.GetGearLocalizationCategory(string defaultCategory)
	{
		return GearLocalizationCategory;
	}

	public virtual List<ItemAffix> GenerateAffixes()
	{
		int count = Item.GetInstanceData().Rarity switch
		{
			ItemRarity.Magic => 2,
			ItemRarity.Rare => 4,
			_ => 0,
		};

		return Affix.GenerateAffixes(AffixHandler.GetAffixes(Item), count);
	}

	public virtual List<ItemAffix> GenerateImplicits()
	{
		return [];
	}

	public virtual void PostRoll() { }
}