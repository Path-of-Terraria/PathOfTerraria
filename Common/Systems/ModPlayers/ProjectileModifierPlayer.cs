using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.ModPlayers;

internal class ProjectileModifierPlayer : ModPlayer
{
	public StatModifier ProjectileSpeedMultiplier { get; set; }
	public StatModifier ProjectileCountModifier { get; set; }

	public override void ResetEffects()
	{
		ProjectileSpeedMultiplier = StatModifier.Default;
		ProjectileCountModifier = StatModifier.Default;
	}

	public override void ModifyShootStats(Item item, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
	{
		if (ProjectileSpeedMultiplier != StatModifier.Default)
		{
			velocity *= ProjectileSpeedMultiplier.ApplyTo(1f);
		}
	}

	public override bool Shoot(Item item, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
	{
		int additionalProjectiles = (int)ProjectileCountModifier.ApplyTo(0f);
		
		if (additionalProjectiles > 0)
		{
			for (int i = 0; i < additionalProjectiles; i++)
			{
				// Slightly randomize angle for any additional projectiles
				float randomAngle = Main.rand.NextFloat(MathHelper.ToRadians(-10), MathHelper.ToRadians(10));

				Vector2 modifiedVelocity = velocity.RotatedBy(randomAngle);
				Projectile.NewProjectile(source, position, modifiedVelocity, type, damage, knockback, Player.whoAmI);
			}
		}

		return base.Shoot(item, source, position, velocity, type, damage, knockback);
	}
}