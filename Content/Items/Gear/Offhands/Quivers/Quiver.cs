using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.BlockSystem;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Core.Items;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Offhands.Quivers;

internal abstract class Quiver : Offhand
{
	protected abstract float MovementSpeedBonus { get; }
	protected abstract float AmmoConsumptionChance { get; }
	protected override string GearLocalizationCategory => "Quiver";

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
		data.ItemType = Common.Enums.ItemType.Quiver;

		InternalDefaults();
	}

	protected virtual void InternalDefaults()
	{
	}

	public override List<ItemAffix> GenerateImplicits()
	{
		return
		[
			(ItemAffix)Affix.CreateAffix<MovementSpeedAffix>(MovementSpeedBonus),
			(ItemAffix)Affix.CreateAffix<AmmoReservationAffix>(AmmoConsumptionChance),
		];
	}
}