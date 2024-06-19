using PathOfTerraria.Content.Projectiles.Whip;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Whip;

internal class LeatherWhip : Whip
{
	public override WhipDrawData DrawData => new(new(10, 26), new(0, 26, 10, 16), new(0, 42, 10, 16), new(0, 58, 10, 16), new(0, 74, 10, 18));

	public override void Defaults()
	{
		Item.DefaultToWhip(ModContent.ProjectileType<WhipBaseProjectile>(), 7, 2, 4);
		Item.channel = true;
	}
}