using PathOfTerraria.Common.Systems.ModPlayers;
using System.Runtime.CompilerServices;
using Terraria.ID;

namespace PathOfTerraria.Common.Projectiles;

// Adjusted from the mod Peculiar Jewelry: https://github.com/GabeHasWon/PeculiarJewelry/blob/master/Content/JewelryMechanic/Stats/Effects/LegionStat.cs

/// <summary>
/// Allows a projectile to be sped up or slowed down arbitrarily.<br/>
/// Use either <see cref="TotalSpeed"/> or <see cref="UniversalBuffingPlayer.UniversalModifier"/>'s 
/// <see cref="Systems.EntityModifier.ProjectileBehaviourSpeed"/> to modify the speed.
/// </summary>
public class SpeedUpProjectile : GlobalProjectile
{
	public override bool InstancePerEntity => true;

	/// <summary>
	/// Determines the boost in 'action speed' for the projectile. This runs AI multiple times when the value is >1.<br/>
	/// This is not reset between frames.<br/>
	/// Use <see cref="AddBehaviourSpeed(float)"/> if you need to modify this and <see cref="VelocityModifier"/>.
	/// </summary>
	internal float TotalSpeed;

	/// <summary>
	/// Determines the boost in 'movement speed' for the projectile. This increases the projectile's movement, accounting for collision, with any >0 value.<br/>
	/// This is reset between frames, and used in PostAI. Ideally, add a value in either PreAI or AI so it takes place properly.<br/>
	/// Use <see cref="AddBehaviourSpeed(float)"/> if you need to modify this and <see cref="TotalSpeed"/>.
	/// </summary>
	internal float VelocityModifier;

	public override void Load()
	{
		On_Projectile.Update += ResetProjectileValues;
	}

	private void ResetProjectileValues(On_Projectile.orig_Update orig, Projectile self, int i)
	{
		if (self.TryGetGlobalProjectile(out SpeedUpProjectile speed))
		{
			speed.VelocityModifier = 0;
		}

		orig(self, i);
	}

	public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
	{
		return entity.minion || entity.sentry;
	}

	public void AddBehaviourSpeed(float speed)
	{
		TotalSpeed += speed;
		VelocityModifier += speed;
	}

	public override bool PreAI(Projectile projectile)
	{
		if (!projectile.TryGetOwner(out Player owner) || owner is null)
		{
			return true;
		}

		float speed = owner.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.ProjectileBehaviourSpeed.Value;
		TotalSpeed += speed;
		VelocityModifier += speed;

		if (TotalSpeed < 0)
		{
			TotalSpeed++;
			projectile.timeLeft++;
			return false;
		}

		while (TotalSpeed > 1f)
		{
			RepeatAI(projectile, (int)TotalSpeed);
			TotalSpeed -= 1;
		}

		return true;
	}

	public override void PostAI(Projectile projectile)
	{
		if (VelocityModifier > 0)
		{
			projectile.velocity *= VelocityModifier;
			HandleMovement(projectile, projectile.velocity * VelocityModifier, out int _, out int _);
			projectile.velocity *= 1 / VelocityModifier;
		}

		[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "HandleMovement")]
		static extern void HandleMovement(Projectile projectile, Vector2 wetVelocity, out int overrideWidth, out int overrideHeight);
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
