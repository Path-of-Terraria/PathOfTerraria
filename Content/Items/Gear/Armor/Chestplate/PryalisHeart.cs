using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.Linq;

namespace PathOfTerraria.Content.Items.Gear.Armor.BodyArmor;

[AutoloadEquip(EquipType.Body)]
internal class PyralisHeart : Chestplate.BodyArmor
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 0.3f;
		staticData.IsUnique = true;
		staticData.BossDropPool = "FireBoss"; // New property for boss-specific drops
		staticData.MinDropItemLevel = 50;
	}

	public override List<ItemAffix> GenerateAffixes()
	{
		return new List<ItemAffix>
		{
			// +150 Maximum Life
			(ItemAffix)Affix.CreateAffix<AddedLifeAffix>(150),
            
			// +20% Fire Resistance  
			(ItemAffix)Affix.CreateAffix<FireResistItemAffix>(20),
            
			// +30 Defense
			(ItemAffix)Affix.CreateAffix<DefenseItemAffix>(30),
            
			// -5% Movement Speed 
			(ItemAffix)Affix.CreateAffix<MovementSpeedAffix>(-5)
		};
	}

	public override void ModifyTooltips(List<TooltipLine> tooltips)
	{
		TooltipLine nameTip = tooltips.First(x => x.Name == "ItemName");

		if (nameTip is not null)
		{
			nameTip.OverrideColor = Color.Orange;
		}
	}

	public override void PostRoll()
	{
		Item.defense = GetItemLevel.Invoke(Item) / 10; 
	}
}