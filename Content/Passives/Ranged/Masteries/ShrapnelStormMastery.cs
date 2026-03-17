using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Projectiles.PassiveProjectiles;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class ShrapnelStormMastery : Passive
{
	internal class ShrapnelStormProjectile : GlobalProjectile
	{
		public override void OnKill(Projectile projectile, int timeLeft)
		{
			if (projectile.TryGetOwner(out Player player) && player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<ShrapnelStormMastery>(out float value) &&
				ProjectileID.Sets.Explosive[projectile.type])
			{
				for (int i = 0; i < 3; ++i)
				{
					Vector2 velocity = new Vector2(0, Main.rand.NextFloat(3, 5)).RotatedByRandom(MathHelper.TwoPi) + projectile.velocity * 0.25f;
					int type = ModContent.ProjectileType<ExplosiveShard>();
					Projectile.NewProjectile(projectile.GetSource_Death(), projectile.Center, velocity, type, (int)(projectile.damage * value / 100f), 4, projectile.owner);
				}
			}
		}
	}
}