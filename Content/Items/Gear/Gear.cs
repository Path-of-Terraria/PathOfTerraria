using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using Terraria.Localization;
using PathOfTerraria.Common.Systems.Affixes;

namespace PathOfTerraria.Content.Items.Gear;

public abstract class Gear : ModItem, GenerateAffixes.IItem, GenerateImplicits.IItem, PostRoll.IItem, GearLocalizationCategory.IItem
{
	// TODO: Figure out how to decouple this logic?

	protected virtual string GearLocalizationCategory => GetType().Name;

	private bool BasicAffixSearchFilter(string key, bool isPrefix)
	{
		return key.StartsWith("Mods.PathOfTerraria.Gear." + GearLocalizationCategory + (isPrefix ? ".Prefixes" : ".Suffixes"));
	}

	public virtual List<ItemAffix> GenerateAffixes()
	{
		return [];
	}

	public virtual List<ItemAffix> GenerateImplicits()
	{
		return [];
	}

	public virtual void PostRoll() { }

	string GearLocalizationCategory.IItem.GetGearLocalizationCategory(string defaultCategory)
	{
		return GearLocalizationCategory;
	}
}