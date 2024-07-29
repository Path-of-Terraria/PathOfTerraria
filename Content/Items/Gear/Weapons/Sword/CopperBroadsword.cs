using PathOfTerraria.Core;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Sword;

internal class CopperBroadsword : Sword
{
	public override float DropChance => 1f;
	public override int MinDropItemLevel => 5;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		GearAlternatives.Register(Type, ItemID.CopperBroadsword);
	}

	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 8;
		Item.width = 42;
		Item.height = 42;
		Item.UseSound = SoundID.Item1;
		ItemType = ItemType.Sword;
	}
}