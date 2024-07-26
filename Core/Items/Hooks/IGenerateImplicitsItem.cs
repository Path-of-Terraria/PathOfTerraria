using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.VanillaInterfaceSystem;
using System.Collections.Generic;

namespace PathOfTerraria.Core.Items.Hooks;

public interface IGenerateImplicitsItem
{
	List<ItemAffix> GenerateImplicits(Item item);

	public static List<ItemAffix> Invoke(Item item)
	{
		if (item.TryGetInterfaces(out IGenerateImplicitsItem[] instances))
		{
			List<ItemAffix> affixes = [];

			foreach (IGenerateImplicitsItem instance in instances)
			{
				affixes.AddRange(instance.GenerateImplicits(item));
			}

			return affixes;
		}

		return [];
	}
}
