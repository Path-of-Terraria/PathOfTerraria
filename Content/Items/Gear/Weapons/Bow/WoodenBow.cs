using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Bow;

internal class WoodenBow : Bow
{
	public override float DropChance => 1f;

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		GearAlternatives.Register(Type, ItemID.WoodenBow);
	}

	public override void SetDefaults()
	{
		Item.damage = 10;
		Item.Size = new Vector2(24, 46);
	}
}