using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.ID;

namespace PathOfTerraria.Common.Projectiles;

// Adjusted from the mod Peculiar Jewelry: https://github.com/GabeHasWon/PeculiarJewelry/blob/master/Content/JewelryMechanic/Stats/Effects/LegionStat.cs

/// <summary>
/// Allows a projectile to be sped up or slowed down arbitrarily.
/// </summary>
public class SpeedUpProjectile : GlobalProjectile
{
	public override bool InstancePerEntity => true;

	float totalSpeed;

	public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
	{
		return entity.minion || entity.sentry;
	}

	public override bool PreAI(Projectile projectile)
	{
		if (!projectile.TryGetOwner(out Player owner) || owner is null)
		{
			return true;
		}

		float speed = owner.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ProjectileBehaviourSpeed.Value;
		totalSpeed += speed;

		if (totalSpeed < 0)
		{
			totalSpeed++;
			projectile.timeLeft++;
			return false;
		}

		while (totalSpeed > 1f)
		{
			RepeatAI(projectile, (int)totalSpeed);
			projectile.position += projectile.velocity;
			totalSpeed -= 1;
		}

		return true;
	}

	public static void RepeatAI(Projectile projectile, int repeats)
	{
		int type = projectile.type;
		bool actType = projectile.ModProjectile != null && projectile.ModProjectile.AIType > ProjectileID.None;

		for (int i = 0; i < repeats; ++i)
		{
			if (actType)
			{
				projectile.type = projectile.ModProjectile.AIType;
			}

			projectile.VanillaAI();

			if (actType)
			{
				projectile.type = type;
			}
		}

		ProjectileLoader.AI(projectile);
	}
}
