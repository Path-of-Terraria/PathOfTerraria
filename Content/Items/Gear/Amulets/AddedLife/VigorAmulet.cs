using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Amulets.AddedLife;

public class VigorAmulet : Amulet
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.value = Item.buyPrice(0, 0, 0, 50);
	}

	public override List<ItemAffix> GenerateImplicits()
	{
		return [(ItemAffix)Affix.CreateAffix<AddedLifeAffix>(3, 6)];
	}
}