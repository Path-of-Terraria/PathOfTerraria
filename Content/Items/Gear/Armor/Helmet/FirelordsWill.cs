using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Head)]
internal class FirelordsWill : Helmet
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 0f;
		staticData.IsUnique = true;
	}

	public override List<ItemAffix> GenerateAffixes()
	{
		return
		[
			(ItemAffix)Affix.CreateAffix<ExtraFireDamage>(30),
			(ItemAffix)Affix.CreateAffix<BaseLifeAffix>(50),
			(ItemAffix)Affix.CreateAffix<CannotBeChilledAffix>(1),
		];
	}

	public override void PostRoll()
	{
		Item.defense = 16;
	}
}
