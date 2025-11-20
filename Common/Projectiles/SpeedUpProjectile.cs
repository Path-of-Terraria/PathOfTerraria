using PathOfTerraria.Common.Systems.ModPlayers;
using Terraria.ID;

namespace PathOfTerraria.Common.Projectiles;

// Adjusted from the mod Peculiar Jewelry: https://github.com/GabeHasWon/PeculiarJewelry/blob/master/Content/JewelryMechanic/Stats/Effects/LegionStat.cs

/// <summary>
/// Allows a projectile to be sped up or slowed down arbitrarily.<br/>
/// Use either <see cref="TotalSpeed"/> or <see cref="UniversalBuffingPlayer.UniversalModifier"/>'s <see cref="Systems.EntityModifier.ProjectileBehaviourSpeed"/> to modify the speed.
/// </summary>
public class SpeedUpProjectile : GlobalProjectile
{
	public override bool InstancePerEntity => true;

	internal float TotalSpeed;

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
		TotalSpeed += speed;

		if (TotalSpeed < 0)
		{
			TotalSpeed++;
			projectile.timeLeft++;
			return false;
		}

		while (TotalSpeed > 1f)
		{
			RepeatAI(projectile, (int)TotalSpeed);
			projectile.position += projectile.velocity;
			TotalSpeed -= 1;
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
