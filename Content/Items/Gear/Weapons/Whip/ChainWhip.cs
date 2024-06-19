using PathOfTerraria.Content.Projectiles.Whip;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Whip;

internal class ChainWhip : Whip
{
	public override WhipDrawData DrawData => new(new(14, 22), new(0, 22, 14, 12), new(0, 34, 14, 12), new(0, 56, 14, 12), new(0, 68, 14, 14));
	public override int ItemLevel => 15;

	public override void Defaults()
	{
		Item.DefaultToWhip(ModContent.ProjectileType<WhipBaseProjectile>(), 13, 2, 4);
		Item.channel = true;
	}
}