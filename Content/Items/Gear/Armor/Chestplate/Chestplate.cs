using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.Items.Hooks;

namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal class Chestplate : Gear, IPostRollItem
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Armor/Body/Base";

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

	public override void PostRoll(Item item)
	{
		Item.defense = IItemLevelControllerItem.GetLevel(item) / 6 + 1;
	}
}