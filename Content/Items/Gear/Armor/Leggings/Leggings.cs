using System.Collections.Generic;
using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.Items.Hooks;
using PathOfTerraria.Core.Systems.Affixes;
using PathOfTerraria.Core.Systems.Affixes.ItemTypes;

namespace PathOfTerraria.Content.Items.Gear.Armor.Leggings;

[AutoloadEquip(EquipType.Legs)]
internal class Leggings : Gear, IGenerateImplicitsItem
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Armor/Legs/Base";

	protected override string GearLocalizationCategory => "Leggings";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		this.GetStaticData().DropChance = 1f;
	}

	public override void SetDefaults()
	{
		this.GetInstanceData().ItemType = ItemType.Leggings;
	}

	public override List<ItemAffix> GenerateImplicits(Item item)
	{
		return [(ItemAffix)Affix.CreateAffix<MovementSpeed>(8)];
	}

	public override void PostRoll(Item item)
	{
		Item.defense = ItemLevel / 12 + 1;
	}
}