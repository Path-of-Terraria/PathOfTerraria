using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

/// <summary>
/// Defines the base class for a chestplate.<br/>
/// Note: You need to manually apply the <see cref="AutoloadEquip"/> attribute for <see cref="EquipType.Body"/>; the attribute can't be inherited and so turns into boilerplate.
/// </summary>
internal abstract class Chestplate : Gear
{
	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Gear/Armor/Body/Base";

	protected override string GearLocalizationCategory => "Chestplate";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Chestplate;
	}

	public override void PostRoll()
	{
		Item.defense = GetItemLevel.Invoke(Item) / 6 + 1;
	}

	public override List<ItemAffix> GenerateAffixes()
	{
		var lifeAffix = (ItemAffix)Affix.CreateAffix<FlatLifeAffix>(25, 35);
		var strengthAffix = (ItemAffix)Affix.CreateAffix<StrengthItemAffix>(15f, 25f);
		var moveAffix = (ItemAffix)Affix.CreateAffix<MovementSpeedAffix>(3, 5);
		var rootedAffix = (ItemAffix)Affix.CreateAffix<ChanceToApplyRootedGearAffix>(3, 5);

		return [lifeAffix, strengthAffix, moveAffix, rootedAffix];
	}
}