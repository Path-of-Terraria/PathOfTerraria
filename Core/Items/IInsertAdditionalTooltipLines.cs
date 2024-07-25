using PathOfTerraria.Core.Systems;
using System.Collections.Generic;

namespace PathOfTerraria.Core.Items;

public interface IInsertAdditionalTooltipLines
{
	void InsertAdditionalTooltipLines(Item item, List<TooltipLine> tooltips, EntityModifier thisItemModifier);
}
