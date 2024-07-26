using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class IronBroadsword : Sword
{

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 1;

		GearAlternatives.Register(Type, ItemID.IronBroadsword);
	}

	public override void SetDefaults()
	{
		Item.damage = 10;
		Item.width = 46;
		Item.height = 46;
		Item.UseSound = SoundID.Item1;
		ItemType = ItemType.Sword;
	}
}