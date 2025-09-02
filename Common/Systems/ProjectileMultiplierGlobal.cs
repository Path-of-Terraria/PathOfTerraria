using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Systems.ProjectileMultiplication;

internal class ProjectileMultiplierGlobal : GlobalProjectile
{
	public override bool InstancePerEntity => true;

	public override void OnSpawn(Projectile projectile, IEntitySource source)
	{
		if (source is EntitySource_Misc misc && misc.Context == "ProjectileMultiplication")
		{
			return;
		}

		if (source is not EntitySource_ItemUse_WithAmmo itemSource)
		{
			return;
		}
		
		// Prevents the following list of weapon projectiles from being +'d. Note that this is only the weapon visual itself, not the projectiles it fires.
		// ProjectileID.LaserMachinegun, ProjectileID.LaserDrill, ProjectileID.ChargedBlasterCannon, ProjectileID.Arkhalis,
		// ProjectileID.PortalGun, ProjectileID.SolarWhipSword, ProjectileID.VortexBeater, ProjectileID.Phantasm, ProjectileID.LastPrism,
		// ProjectileID.DD2PhoenixBow, ProjectileID.Celeb2Weapon, ProjectileID.Terragrim, ProjectileID.PiercingStarlight
		if (projectile.aiStyle == 75)
		{
			return;
		}

		Player player = itemSource.Player;
		ProjectileModifierPlayer modPlayer = player.GetModPlayer<ProjectileModifierPlayer>();

		// Skip if no modifier applied
		if (modPlayer.ProjectileCountModifier == StatModifier.Default)
		{
			return;
		}

		int additionalProjectiles = (int)modPlayer.ProjectileCountModifier.ApplyTo(0f);
		{
			if (additionalProjectiles <= 0)
				return;
		}

		// Use EntitySource_Misc to prevent infinite recursion
		EntitySource_Misc multiSource = new("ProjectileMultiplication");

		for (int i = 0; i < additionalProjectiles; i++)
		{
			float randomAngle = Main.rand.NextFloat(MathHelper.ToRadians(-10), MathHelper.ToRadians(10));
			Vector2 modifiedVelocity = projectile.velocity.RotatedBy(randomAngle);
			Projectile.NewProjectile(multiSource, projectile.position, modifiedVelocity, projectile.type, projectile.damage, projectile.knockBack, player.whoAmI);
		}
	}
}