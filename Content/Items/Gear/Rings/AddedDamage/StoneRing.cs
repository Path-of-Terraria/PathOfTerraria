using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Rings.AddedDamage;

public class StoneRing : Ring
{
	public override List<ItemAffix> GenerateImplicits()
	{
		return [(ItemAffix)Affix.CreateAffix<AddedDamageAffix>(-1, 2, 3)];
	}
}