using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Talismans;

internal abstract class Talisman : Offhand
{
	protected abstract float SummonDamage { get; }
	protected override string GearLocalizationCategory => "Talisman";

	public override void SetStaticDefaults()
	{
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 1;
	}

	public override void SetDefaults()
	{
		Item.accessory = true;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = Common.Enums.ItemType.Talisman;

		InternalDefaults();
	}

	protected virtual void InternalDefaults()
	{
	}
	
	public override List<ItemAffix> GenerateImplicits()
	{
		return
		[
			(ItemAffix)Affix.CreateAffix<IncreasedSummonDamageAffix>(SummonDamage),
		];
	}
}