using PathOfTerraria.Core.Systems;
using System.Collections.Generic;

namespace PathOfTerraria.Core.Items;

public interface IInsertAdditionalTooltipLinesItem
{
	void InsertAdditionalTooltipLines(Item item, List<TooltipLine> tooltips, EntityModifier thisItemModifier);
}
