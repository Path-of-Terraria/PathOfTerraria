using PathOfTerraria.Core;
using PathOfTerraria.Core.Items.Hooks;

namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal class Chestplate : Gear, IPostRollItem
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Armor/Body/Base";

	protected override string GearLocalizationCategory => "Chestplate";
	public override float DropChance => 1f;

	public override void SetDefaults()
	{
		ItemType = ItemType.Chestplate;
	}

	public override void PostRoll(Item item)
	{
		Item.defense = ItemLevel / 6 + 1;
	}
}