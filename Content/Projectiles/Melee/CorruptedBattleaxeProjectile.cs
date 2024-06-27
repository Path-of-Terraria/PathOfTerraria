namespace PathOfTerraria.Content.Projectiles.Melee;

public class CorruptedBattleaxeProjectile : ModProjectile
{
	public override string Texture => $"{PathOfTerraria.ModName}/Assets/Items/Gear/Weapons/Battleaxe/CorruptedBattleaxe";
	
	public override void SetDefaults()
	{
		Projectile.width = 52;
		Projectile.height = 52;
		Projectile.friendly = true;
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.timeLeft = 300; 
	}

	/// <summary>
	/// Redirects the projectile towards the closest NPC.
	/// </summary>
	public override void AI()
	{
		Projectile.rotation += 0.1f;
		
		NPC closestNpc = FindClosestNpc(500f);
		if (closestNpc == null)
		{
			return;
		}

		Vector2 direction = closestNpc.Center - Projectile.Center;
		direction.Normalize();
		direction *= 10f;
		Projectile.velocity = (Projectile.velocity * 20f + direction) / 21f;
	}

	private NPC FindClosestNpc(float maxDetectDistance)
	{
		NPC closestNpc = null;
		float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;
		float closestDist = sqrMaxDetectDistance;

		for (int i = 0; i < Main.maxNPCs; i++)
		{
			NPC target = Main.npc[i];
			if (!target.CanBeChasedBy(this))
			{
				continue;
			}

			float sqrDistToTarget = Vector2.DistanceSquared(target.Center, Projectile.Center);
			if (!(sqrDistToTarget < closestDist))
			{
				continue;
			}

			closestDist = sqrDistToTarget;
			closestNpc = target;
		}

		return closestNpc;
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		Projectile.Kill();
		return false;
	}

	/// <summary>
	/// When the projectile hits an NPC, find another target nearby and redirect the projectile.
	/// </summary>
	/// <param name="target"></param>
	/// <param name="hit"></param>
	/// <param name="damageDone"></param>
	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		// After hitting a target, find another target nearby and redirect the projectile
		NPC newTarget = FindClosestNpc(300f); // Adjust the range for finding the next target
		if (newTarget != null)
		{
			Vector2 direction = newTarget.Center - Projectile.Center;
			direction.Normalize();
			direction *= 10f;
			Projectile.velocity = direction;
		}
		else
		{
			Projectile.Kill(); // No more targets, kill the projectile
		}
	}
}