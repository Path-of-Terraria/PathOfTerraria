using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.ID;

namespace PathOfTerraria.Common.Projectiles;

internal class ExplosiveSizeProjectile : GlobalProjectile
{
	public override void Load()
	{
		On_Projectile.Resize += ResizeExplosive;
		On_Projectile.ExplodeTiles += IncreaseExplosiveRange;
	}

	private void IncreaseExplosiveRange(On_Projectile.orig_ExplodeTiles orig, Projectile self, Vector2 compareSpot, int radius, int minI, int maxI, int minJ, int maxJ, bool wall)
	{
		if (self.TryGetOwner(out Player player))
		{
			Point center = compareSpot.ToTileCoordinates();

			int oldRadius = radius;
			EntityModifier modifier = player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier;
			float areaScale = CustomProjectileSets.AreaOfEffectProjectiles[self.type]
				? modifier.AreaOfEffect.ApplyTo(1f)
				: 1f;
			radius = (int)(modifier.ExplosionSize.ApplyTo(radius) * areaScale);

			int delta = radius - oldRadius;
			minI -= delta;
			minJ -= delta;
			maxI += delta;
			maxJ += delta;
		}

		orig(self, compareSpot, radius, minI, maxI, minJ, maxJ, wall);
	}

	private void ResizeExplosive(On_Projectile.orig_Resize orig, Projectile self, int newWidth, int newHeight)
	{
		if (ProjectileID.Sets.Explosive[self.type] && self.TryGetOwner(out Player player))
		{
			EntityModifier modifier = player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier;
			float areaScale = CustomProjectileSets.AreaOfEffectProjectiles[self.type]
				? modifier.AreaOfEffect.ApplyTo(1f)
				: 1f;
			float size = modifier.ExplosionSize.ApplyTo(1f) * areaScale;
			newWidth = (int)(newWidth * size);
			newHeight = (int)(newHeight * size);
		}

		orig(self, newWidth, newHeight);
	}

}
