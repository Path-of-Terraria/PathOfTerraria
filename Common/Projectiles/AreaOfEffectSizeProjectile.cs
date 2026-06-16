using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Passives;
using Terraria.ID;

namespace PathOfTerraria.Common.Projectiles;

internal class AreaOfEffectSizeProjectile : GlobalProjectile
{
	public override bool InstancePerEntity => true;

	private bool _scaled;

	public override void PostAI(Projectile projectile)
	{
		TryApplyScaling(projectile);
	}

	private void TryApplyScaling(Projectile projectile)
	{
		if (_scaled || !projectile.active)
		{
			return;
		}

		_scaled = true;

		if (!projectile.friendly)
		{
			return;
		}

		if (!projectile.TryGetOwner(out Player owner))
		{
			return;
		}

		if (!ShouldScale(projectile))
		{
			return;
		}

		PassiveTreePlayer passiveTreePlayer = owner.GetModPlayer<PassiveTreePlayer>();
		float passiveValue = passiveTreePlayer.GetCumulativeValue<BiggerExplosivesPassive>()
			+ passiveTreePlayer.GetCumulativeValue<IncreasedAreaOfEffectPassive>();

		if (passiveValue <= 0f)
		{
			return;
		}

		float scaleMultiplier = 1f + passiveValue / 100f;

		int originalWidth = projectile.width;
		int originalHeight = projectile.height;
		Vector2 center = projectile.Center;
		projectile.scale *= scaleMultiplier;
		projectile.Resize(Math.Max(1, (int)(originalWidth * scaleMultiplier)), Math.Max(1, (int)(originalHeight * scaleMultiplier)));
		projectile.Center = center;
		projectile.netUpdate = true;
	}

	private static bool ShouldScale(Projectile projectile)
	{
		return CustomProjectileSets.AreaOfEffectProjectiles[projectile.type]
			|| ProjectileID.Sets.Explosive[projectile.type]
			|| ProjectileID.Sets.IsAWhip[projectile.type]
			|| ProjectileID.Sets.MinionShot[projectile.type]
			|| projectile.minion
			|| projectile.sentry
			|| projectile.DamageType.CountsAsClass(DamageClass.Summon);
	}
}
