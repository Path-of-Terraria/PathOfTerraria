using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class CopperBroadsword : Sword
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 5;

		GearAlternatives.Register(Type, ItemID.CopperBroadsword);
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 8;
		Item.width = 42;
		Item.height = 42;
		Item.UseSound = SoundID.Item1;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Sword;
	}
}