using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Focuses;

internal abstract class Focus<T> : Offhand where T : ItemAffix
{
	protected abstract float Strength { get; }
	protected override string GearLocalizationCategory => "Focus";

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
		data.ItemType = Common.Enums.ItemType.Focus;

		InternalDefaults();
	}

	protected virtual void InternalDefaults()
	{
	}

	public override List<ItemAffix> GenerateImplicits()
	{
		return
		[
			(ItemAffix)Affix.CreateAffix<T>(Strength),
		];
	}
}
