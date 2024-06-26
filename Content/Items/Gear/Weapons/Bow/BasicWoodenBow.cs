using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Bow;

internal class BasicWoodenBow : Bow
{
	public override float DropChance => 1f;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();
		GearAlternatives.Register(Type, ItemID.WoodenBow);
	}

	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 8;
		Item.Size = new Vector2(24, 42);
	}
}