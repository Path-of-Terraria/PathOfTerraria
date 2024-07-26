using PathOfTerraria.Core.Systems.Affixes;
using System.Collections.Generic;

namespace PathOfTerraria.Core.Items.Hooks;

public interface IGenerateAffixesItem
{
	List<ItemAffix> GenerateAffixes();

	public static List<ItemAffix> Invoke(Item item)
	{
		if (item.ModItem is IGenerateAffixesItem generateAffixesItem)
		{
			return generateAffixesItem.GenerateAffixes();
		}

		return [];
	}
}
