using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class FallenKingsLegacy : Leggings
{
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
	}

	public override List<ItemAffix> GenerateAffixes()
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
