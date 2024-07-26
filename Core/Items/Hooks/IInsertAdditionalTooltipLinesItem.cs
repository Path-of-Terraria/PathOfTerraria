using PathOfTerraria.Core.Systems;
using PathOfTerraria.Core.Systems.VanillaInterfaceSystem;
using System.Collections.Generic;

namespace PathOfTerraria.Core.Items.Hooks;

public interface IInsertAdditionalTooltipLinesItem
{
	void InsertAdditionalTooltipLines(Item item, List<TooltipLine> tooltips, EntityModifier thisItemModifier);

	public static void Invoke(Item item, List<TooltipLine> tooltips, EntityModifier thisItemModifier)
	{
		if (item.TryGetInterfaces(out IInsertAdditionalTooltipLinesItem[] instances))
		{
			foreach (IInsertAdditionalTooltipLinesItem instance in instances)
			{
				instance.InsertAdditionalTooltipLines(item, tooltips, thisItemModifier);
			}
		}
	}
}