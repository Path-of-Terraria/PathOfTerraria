using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Rings.AddedDamage;

public class StoneRing : Ring
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.value = Item.buyPrice(0, 0, 0, 15);
	}

	public override List<ItemAffix> GenerateImplicits()
	{
		return [(ItemAffix)Affix.CreateAffix<AddedDamageAffix>(2, 3)];
	}
}