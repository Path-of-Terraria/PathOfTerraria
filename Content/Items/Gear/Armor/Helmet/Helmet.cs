using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Items.Gear.Armor.Helmet;

[AutoloadEquip(EquipType.Head)]
internal class Helmet : Gear
{
	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Gear/Armor/Helmet/Base";

	protected override string GearLocalizationCategory => "Helmet";

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1232f;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();
		
		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Helmet;
	}

	public override void PostRoll()
	{
		this.GetInstanceData().Affixes.Add(Affix.CreateAffix<MultipliedLifeAffix>(30) as ItemAffix);
		Item.defense = GetItemLevel.Invoke(Item) / 10 + 1;
	}
}