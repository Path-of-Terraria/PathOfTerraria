using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using Terraria.Localization;
using PathOfTerraria.Common.Systems.Affixes;

namespace PathOfTerraria.Content.Items.Gear;

public abstract class Gear : ModItem, GenerateAffixes.IItem, GenerateImplicits.IItem, GeneratePrefix.IItem, GenerateSuffix.IItem, PostRoll.IItem
{
	// TODO: Figure out how to decouple this logic?

	protected virtual string GearLocalizationCategory => GetType().Name;

	/// <summary>
	/// Selects a prefix to be added to the name of the item from the provided Prefixes in localization files
	/// </summary>
	/// <returns></returns>
	public virtual string GeneratePrefix(string defaultPrefix)
	{
		return Language.SelectRandom((key, _) => BasicAffixSearchFilter(key, true)).Value;
	}

	/// <summary>
	/// Selects a suffix to be added to the name of the item from the provided Suffixes in localization files
	/// </summary>
	/// <returns></returns>
	public virtual string GenerateSuffix(string defaultSuffix)
	{
		return Language.SelectRandom((key, _) => BasicAffixSearchFilter(key, false)).Value;
	}

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
}