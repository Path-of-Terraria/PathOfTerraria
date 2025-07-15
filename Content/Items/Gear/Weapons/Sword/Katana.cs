using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class Katana : Sword
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		
		PoTStaticItemData staticData = this.GetStaticData();
		staticData.DropChance = 1f;
		staticData.MinDropItemLevel = 15;

		GearAlternatives.Register(Type, Terraria.ID.ItemID.Katana);
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.damage = 20;
		Item.useTime = 35; 
		Item.useAnimation = 35;
		Item.width = 48; 
		Item.height = 54;
		Item.value = Item.buyPrice(0, 0, 1, 50);
	}
}