using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

/// <summary>
/// Defines the base class for a helmet.<br/>
/// Note: You need to manually apply the <see cref="AutoloadEquip"/> attribute for <see cref="EquipType.Head"/>; the attribute can't be inherited and so turns into boilerplate.
/// </summary>
internal abstract class Helmet : Gear
{
	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Gear/Armor/Helmet/Base";

	protected override string GearLocalizationCategory => "Helmet";

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
		data.ItemType = ItemType.Helmet;
	}

	public override void PostRoll()
	{
		Item.defense = GetItemLevel.Invoke(Item) / 10 + 1;
	}
}