using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Shields;

internal abstract class Shield : Offhand
{
	protected abstract float BlockChance { get; }
	protected abstract float SpeedReduction { get; }
	protected override string GearLocalizationCategory => "Shield";

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
		data.ItemType = Common.Enums.ItemType.Shield;

		InternalDefaults();
	}

	protected virtual void InternalDefaults()
	{
	}
	
	public override List<ItemAffix> GenerateImplicits()
	{
		return
		[
			(ItemAffix)Affix.CreateAffix<MovementSpeedAffix>(-SpeedReduction),
			(ItemAffix)Affix.CreateAffix<AddBlockAffix>(BlockChance * 100),
		];
	}
}
