using Terraria.ID;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Bow;

internal class WoodenBow : Bow
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Bow/WoodenBow";
	public override float DropChance => 1f;

	public override void SetStaticDefaults()
	{
		GearAlternatives.Register(Type, ItemID.WoodenBow);
	}

	public override void Defaults()
	{
		base.Defaults();
		Item.damage = 10;
	}
}