using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Melee;

public class CorruptedBattleaxeProjectile : ModProjectile
{
	private bool _targetingPlayer;
	
	/// <summary>
	/// The distance it will look to target the next entity when bouncing
	/// </summary>
	private readonly float _distance = 500f;

	public override string Texture => $"{PoTMod.ModName}/Assets/Items/Gear/Weapons/Battleaxe/CorruptedBattleaxe";
        
	public override void SetDefaults()
	{
		Projectile.width = 52;
		Projectile.height = 52;
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		Projectile.ignoreWater = true;
		Projectile.timeLeft = 300; 
	}

	public override void AI()
	{
		Projectile.rotation += 0.2f;

		if (Projectile.timeLeft < 60)
		{
			Projectile.Opacity = Projectile.timeLeft / 60f;
		}
		
		if (_targetingPlayer)
		{
			Player player = FindClosestPlayer(_distance);

			if (player == null)
			{
				return;
			}
			
			Vector2 direction = Projectile.DirectionTo(player.Center) * 10;
			Projectile.friendly = false;
			Projectile.hostile = true; 
			Projectile.velocity = direction;
		}
		else
		{
			NPC closestNpc = FindClosestNpc(_distance);

			if (closestNpc == null)
			{
				return;
			}

			Vector2 direction = Projectile.DirectionTo(closestNpc.Center) * 10;
			Projectile.velocity = (Projectile.velocity * 20f + direction) / 21f;	
		}
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

	public override void OnHitPlayer(Player target, Player.HurtInfo info)
	{
		Projectile.damage *= 2;
		_targetingPlayer = false;

		NPC newTarget = FindClosestNpc(_distance);

		if (newTarget != null)
		{
			Vector2 direction = Projectile.DirectionTo(newTarget.Center) * 10;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.velocity = direction;
		}
		else
		{
			Projectile.Kill(); // No more targets, kill the projectile

			for (int i = 0; i < 5; ++i)
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Demonite, Projectile.velocity.X, Projectile.velocity.Y);
			}
		}
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		// 1% chance to target the player
		if (Main.rand.NextBool(100))
		{
			Player player = FindClosestPlayer(_distance);
			if (player != null)
			{
				_targetingPlayer = true;
				Projectile.damage /= 2;
				return;
			}
		}	

		// After hitting a target, find another target nearby and redirect the projectile
		NPC newTarget = FindClosestNpc(_distance);
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

	private Player FindClosestPlayer(float maxDetectDistance)
	{
		Player closestPlayer = null;
		float sqrMaxDetectDistance = maxDetectDistance * maxDetectDistance;
		float closestDist = sqrMaxDetectDistance;

		foreach (Player player in Main.player)
		{
			if (!player.active || player.dead)
			{
				continue;
			}

			float sqrDistToPlayer = Vector2.DistanceSquared(player.Center, Projectile.Center);
			if (!(sqrDistToPlayer < closestDist))
			{
				continue;
			}

			closestDist = sqrDistToPlayer;
			closestPlayer = player;
		}

		return closestPlayer;
	}
}