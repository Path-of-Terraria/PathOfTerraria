using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Rings;

public class ProlifRing : Ring
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.value = Item.buyPrice(0, 1, 0, 0);

		this.GetInstanceData().Rarity = ItemRarity.Rare;
	}

	public override List<ItemAffix> GenerateImplicits()
	{
		return
		[
			(ItemAffix)Affix.CreateAffix<SpreadIgniteScorchOnKillAffix>(1),
			(ItemAffix)Affix.CreateAffix<AllResistancesAffix>(20),
			(ItemAffix)Affix.CreateAffix<ExtraFireDamage>(20),
			(ItemAffix)Affix.CreateAffix<BaseLifeAffix>(40),
		];
	}
}
