using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.BlockSystem;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Shields;

internal abstract class Shield : Offhand
{
	internal abstract float BaseBlockChance { get; }
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

	public override void UpdateAccessory(Player player, bool hideVisual)
	{
		player.GetModPlayer<BlockPlayer>().AddBlockChance(BaseBlockChance);
	}

	public override void PostRoll()
	{
		base.PostRoll();

		PoTInstanceItemData data = Item.GetInstanceData();
		int removedImplicits = data.Affixes.RemoveAll(affix => affix is AddBlockAffix && affix.IsImplicit);
		data.ImplicitCount = Math.Max(0, data.ImplicitCount - removedImplicits);
	}
	
	public override List<ItemAffix> GenerateImplicits()
	{
		return
		[
			(ItemAffix)Affix.CreateAffix<MovementSpeedAffix>(-SpeedReduction),
		];
	}
}
