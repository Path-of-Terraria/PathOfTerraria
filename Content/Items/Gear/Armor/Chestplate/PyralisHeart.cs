using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal class PyralisHeart : Chestplate
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = null;
		staticData.IsUnique = true;
	}

	public override List<ItemAffix> GenerateAffixes()
	{
		return
		[
			(ItemAffix)Affix.CreateAffix<BaseLifeAffix>(150),
			(ItemAffix)Affix.CreateAffix<FireResistItemAffix>(20),
			(ItemAffix)Affix.CreateAffix<MovementSpeedAffix>(-5),
		];
	}

	public override void PostRoll()
	{
		Item.defense = 30;
	}
}
