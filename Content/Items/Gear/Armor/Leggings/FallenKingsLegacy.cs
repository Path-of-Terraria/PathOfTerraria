using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class FallenKingsLegacy : Leggings
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		this.GetInstanceData().Rarity = ItemRarity.Magic;
	}

	public override List<ItemAffix> GenerateImplicits()
	{
		return
		[
			(ItemAffix)Affix.CreateAffix<NearbyEnemiesScorchedAffix>(1),
			(ItemAffix)Affix.CreateAffix<PermanentlyScorchedAffix>(1),
			(ItemAffix)Affix.CreateAffix<ExtraFireDamage>(20),
			(ItemAffix)Affix.CreateAffix<MovementSpeedAffix>(20),
			(ItemAffix)Affix.CreateAffix<IncreasedIgniteEffectAffix>(50),
		];
	}

	public override void PostRoll()
	{
		Item.defense = 8;
	}
}
