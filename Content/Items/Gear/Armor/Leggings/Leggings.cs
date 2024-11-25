using System.Collections.Generic;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

internal class Leggings : Gear
{
	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Gear/Armor/Legs/Base";

	protected override string GearLocalizationCategory => "Leggings";

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
		data.ItemType = ItemType.Leggings;
	}

	public override List<ItemAffix> GenerateImplicits()
	{
		return [(ItemAffix)Affix.CreateAffix<MovementSpeed>(8)];
	}

	public override void PostRoll()
	{
		Item.defense = GetItemLevel.Invoke(Item) / 12 + 1;
	}
}