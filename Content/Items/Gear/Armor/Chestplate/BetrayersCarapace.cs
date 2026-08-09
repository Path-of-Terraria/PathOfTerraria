using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal sealed class BetrayersCarapace : Chestplate
{
	public override string Texture => $"{PoTMod.ModName}/Content/Items/Gear/Armor/Body/SpectralShroud";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = null;
		staticData.IsUnique = true;
	}

	public override List<ItemAffix> GenerateAffixes() =>
	[
		(ItemAffix)Affix.CreateAffix<BaseLifeAffix>(150),
		(ItemAffix)Affix.CreateAffix<ColdResistItemAffix>(20),
		(ItemAffix)Affix.CreateAffix<MovementSpeedAffix>(-5),
	];

	public override void PostRoll()
	{
		Item.defense = 30;
	}
}
