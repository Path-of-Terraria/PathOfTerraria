using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Bow;

internal class WardensBow : WoodenBow
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = null;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 40;
		Item.Size = new Vector2(24, 48);
	}

	public override List<ItemAffix> GenerateAffixes()
	{
		var addedDamageAffix = (ItemAffix)Affix.CreateAffix<AddedLifeAffix>(25, 35);
		var increasedDamageAffix = (ItemAffix)Affix.CreateAffix<IncreasedDamageAffix>(15f, 25f);
		var attackSpeedAffix = (ItemAffix)Affix.CreateAffix<IncreasedAttackSpeedAffix>(0.1f);
		var armorShredAffix = (ItemAffix)Affix.CreateAffix<ChanceToApplyArmorShredGearAffix>(0.1f, 0.2f);
		armorShredAffix.Duration = 120;
		return [addedDamageAffix, increasedDamageAffix, attackSpeedAffix, armorShredAffix];
	}
}