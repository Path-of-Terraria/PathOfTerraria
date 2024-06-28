using PathOfTerraria.Content.Projectiles.Whip;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Whip;

internal abstract class Whip : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Whip/WhipPlaceholder";
	public override float DropChance => 1f;

	protected override string GearLocalizationCategory => "Whip";

	public override void Defaults()
	{
		Item.DefaultToWhip(ModContent.ProjectileType<WhipBaseProjectile>(), 10, 2, 4);
		Item.channel = true;
	}
}