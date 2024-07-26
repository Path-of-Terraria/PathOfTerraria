using PathOfTerraria.Core;
using PathOfTerraria.Core.Items;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class WoodenSword : Sword
{
	public override float DropChance => 1f;
	public override int ItemLevel => 1;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		GearAlternatives.Register(Type, ItemID.WoodenSword);
	}

	public override void SetDefaults()
	{
		Item.Size = new(38);
		Item.damage = 4;
		Item.UseSound = SoundID.Item1;

		PoTInstanceItemData data = this.GetInstanceData();
		data.ItemType = ItemType.Sword;
	}
}