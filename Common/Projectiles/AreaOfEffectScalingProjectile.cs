using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.DataStructures;

namespace PathOfTerraria.Common.Projectiles;

internal class AreaOfEffectScalingProjectile : GlobalProjectile
{
	private const float ScaleEpsilon = 0.0001f;

	public override void OnSpawn(Projectile projectile, IEntitySource source)
	{
		if (!CustomProjectileSets.AreaOfEffectProjectiles[projectile.type])
		{
			return;
		}

		if (!projectile.TryGetOwner(out Player owner))
		{
			return;
		}

		float scale = owner.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.AreaOfEffect.ApplyTo(1f);
		if (MathF.Abs(scale - 1f) < ScaleEpsilon)
		{
			return;
		}

		projectile.scale *= scale;
		projectile.Size = projectile.Size * scale;
	}
}
