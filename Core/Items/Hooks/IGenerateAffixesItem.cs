using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.VanillaInterfaceSystem;
using System.Collections.Generic;

namespace PathOfTerraria.Core.Items.Hooks;

public interface IGenerateAffixesItem
{
	List<ItemAffix> GenerateAffixes(Item item);

	public static List<ItemAffix> Invoke(Item item)
	{
		if (item.TryGetInterfaces(out IGenerateAffixesItem[] instances))
		{
			List<ItemAffix> affixes = [];

			foreach (IGenerateAffixesItem instance in instances)
			{
				affixes.AddRange(instance.GenerateAffixes(item));
			}

			return affixes;
		}

		return [];
	}
}
