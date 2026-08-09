using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Rings;

internal sealed class FrozenOrbit : Ring
{
	public override string Texture => $"{PoTMod.ModName}/Content/Items/Gear/Rings/ProlifRing";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = null;
		staticData.IsUnique = true;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.value = Item.buyPrice(gold: 1);
	}

	public override List<ItemAffix> GenerateImplicits() =>
	[
		(ItemAffix)Affix.CreateAffix<BaseLifeAffix>(8, 12),
	];

	public override List<ItemAffix> GenerateAffixes() =>
	[
		(ItemAffix)Affix.CreateAffix<AllResistancesAffix>(20),
		(ItemAffix)Affix.CreateAffix<ExtraColdDamage>(20),
		(ItemAffix)Affix.CreateAffix<BaseLifeAffix>(40),
		(ItemAffix)Affix.CreateAffix<CannotBeChilledAffix>(1),
	];
}
