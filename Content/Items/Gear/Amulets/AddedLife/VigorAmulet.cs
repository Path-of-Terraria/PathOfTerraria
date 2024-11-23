using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Amulets.AddedLife;

public class VigorAmulet : Amulet
{
	public override List<ItemAffix> GenerateImplicits()
	{
		return [(ItemAffix)Affix.CreateAffix<AddedLifeAffix>(-1, 3, 6)];
	}
}