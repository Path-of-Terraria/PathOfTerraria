using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Chestplate;

[AutoloadEquip(EquipType.Body)]
internal class Chestplate : Gear
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
}