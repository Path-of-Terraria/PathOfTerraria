using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Common.Systems.ProjectileMultiplication;

internal class ProjectileMultiplierGlobal : GlobalProjectile
{
	public override bool InstancePerEntity => true;

	public override void OnSpawn(Projectile projectile, IEntitySource source)
	{
		if (source is not EntitySource_ItemUse_WithAmmo itemSource)
		{
			return;
		}
		
		// Skip held weapon projectiles using ProjAIStyleID constants. Reference: https://docs.tmodloader.net/docs/1.4-preview/class_terraria_1_1_i_d_1_1_proj_a_i_style_i_d.html
		if (projectile.aiStyle == ProjAIStyleID.HeldProjectile)
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
		
		EntitySource_Misc multiSource = new("ProjectileMultiplication");

		for (int i = 0; i < additionalProjectiles; i++)
		{
			float randomAngle = Main.rand.NextFloat(MathHelper.ToRadians(-10), MathHelper.ToRadians(10));
			Vector2 modifiedVelocity = projectile.velocity.RotatedBy(randomAngle);
			Projectile.NewProjectile(multiSource, projectile.position, modifiedVelocity, projectile.type, projectile.damage, projectile.knockBack, player.whoAmI);
		}
	}
}