using PathOfTerraria.Common.Systems.ElementalDamage;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.PassiveProjectiles;

internal sealed class ChaosDevilsknifeScytheProjectile : ModProjectile
{
	public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.DemonScythe}";

	public override void SetDefaults()
	{
		Projectile.CloneDefaults(ProjectileID.DemonScythe);
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		Projectile.timeLeft = 4 * 60;
		Projectile.DamageType = DamageClass.Generic;
	}

	public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
	{
		if (Projectile.TryGetGlobalProjectile(out ElementalProjectile elementalProjectile))
		{
			elementalProjectile.AddElementalValues((ElementType.Chaos, 0, 1f));
		}
	}
}
