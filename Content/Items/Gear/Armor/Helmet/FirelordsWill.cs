using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Head)]
internal class FirelordsWill : Helmet
{
	public override List<ItemAffix> GenerateImplicits()
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
