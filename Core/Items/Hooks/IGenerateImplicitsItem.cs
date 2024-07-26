using PathOfTerraria.Core.Systems.Affixes;
using System.Collections.Generic;

namespace PathOfTerraria.Core.Items.Hooks;

public interface IGenerateImplicitsItem
{
	List<ItemAffix> GenerateImplicits(Item item);

	public static List<ItemAffix> Invoke(Item item)
	{
		if (item.ModItem is IGenerateImplicitsItem generateImplicitsItem)
		{
			return generateImplicitsItem.GenerateImplicits(item);
		}

		return [];
	}
}
