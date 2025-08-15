using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using Terraria.Localization;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class EightAllianceStride : Leggings, GenerateName.IItem
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 0.01f;
		staticData.IsUnique = true;
	}
	
	public override void UpdateEquip(Player player)
	{
		player.maxRunSpeed += 4f; // This allows much higher speeds
		
		base.UpdateEquip(player);
	}

	public override List<ItemAffix> GenerateImplicits()
	{
		// 50% ms only
		return [(ItemAffix)Affix.CreateAffix<MovementSpeedAffix>(50f)];
	}
	
	string GenerateName.IItem.GenerateName(string defaultName)
	{
		return Language.GetTextValue("Mods.PathOfTerraria.Items.EightAllianceStride.DisplayName");
	}

	public override List<ItemAffix> GenerateAffixes()
	{
		return [];
	}
}
