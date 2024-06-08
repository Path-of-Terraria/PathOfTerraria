using PathOfTerraria.Content.Projectiles.Whip;

namespace PathOfTerraria.Content.Items.Gear.Weapons.Whip;

internal abstract class Whip : Gear
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Whip/WhipPlaceholder";

	public override float DropChance => 1f;

	public override void SetDefaults()
	{
		Item.DefaultToWhip(ModContent.ProjectileType<WhipBaseProjectile>(), 10, 2, 4);
		Item.channel = true;
	}

	public override string GeneratePrefix()
	{
		return Main.rand.Next(5) switch
		{
			0 => "Searing",
			1 => "Venomous",
			2 => "Barbed",
			3 => "Fiery",
			4 => "Thunderous",
			_ => "Unknown"
		};
	}

	public override string GenerateSuffix()
	{
		return Main.rand.Next(5) switch
		{
			0 => "Lash",
			1 => "Fang",
			2 => "Scourge",
			3 => "Strike",
			4 => "Snap",
			_ => "Unknown"
		};
	}
}