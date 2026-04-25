using PathOfTerraria.Common;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.PassiveProjectiles;

internal class RagingSpirit : ModProjectile
{
	private bool HasTarget
	{
		get => Projectile.ai[0] == 1;
		set => Projectile.ai[0] = value ? 1 : 0;
	}

	private int Target => (int)Projectile.ai[1];

	public override void SetStaticDefaults()
	{
		ProjectileID.Sets.MinionShot[Type] = true;

		Main.projFrames[Type] = 11;
	}

	public override void SetDefaults()
	{
		Projectile.Size = new Vector2(24, 24);
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.timeLeft = 5 * 60;
		Projectile.minion = true;
		Projectile.DamageType = DamageClass.Summon;
		Projectile.tileCollide = false;
		Projectile.penetrate = 3;
		Projectile.Opacity = 1f;
		Projectile.scale = 1f;
	}

	public override void AI()
	{
		Projectile.frameCounter++;
		Projectile.frame = (int)(Projectile.frameCounter * 0.2f % 11);

		if (!HasTarget)
		{
			int target = -1;
			float targetSqrDist = float.PositiveInfinity;
			Vector2 center = Projectile.Center;

			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.CanBeChasedBy() && npc.DistanceSQ(center) is float sqrDist and < 500 * 500 && sqrDist < targetSqrDist)
				{
					target = npc.whoAmI;
					targetSqrDist = sqrDist;
				}
			}

			if (target != -1)
			{
				HasTarget = true;
				Projectile.ai[1] = target;
			}
		}

		if (!HasTarget)
		{
			return;
		}

		NPC targetNPC = Main.npc[Target];

		if (Projectile.timeLeft > 15)
		{
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.SafeDirectionTo(targetNPC.Center) * 8, 0.02f);
		}
		else
		{
			Projectile.velocity *= 0.9f;
		}
	}

	public override void OnKill(int timeLeft)
	{
		for (int i = 0; i < 18; ++i)
		{
			Vector2 vel = Main.rand.NextVector2Circular(4, 4);
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Ash, vel.X, vel.Y);
		}
	}
}
