using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Rings.AddedLife;

public class FireDiamondRing : Ring
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.value = Item.buyPrice(0, 0, 0, 25);
	}

	public override List<ItemAffix> GenerateImplicits()
	{
		return [(ItemAffix)Affix.CreateAffix<BaseLifeAffix>(13, 16)];
	}
}