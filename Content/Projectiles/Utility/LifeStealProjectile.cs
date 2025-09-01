using System.Security.Permissions;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Utility;

public class LifeStealProjectile : ModProjectile
{
	public override void SetStaticDefaults()
	{
		Main.projFrames[Projectile.type] = 7;
	}

	public override void SetDefaults()
	{
		Projectile.width = 36;
		Projectile.height = 36;
		Projectile.friendly = true;
		Projectile.DamageType = DamageClass.Melee;
		Projectile.ignoreWater = true;
		Projectile.tileCollide = false;
		Projectile.Opacity = 0;
		Projectile.penetrate = 2;
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return new Color(255, 255, 255, 0) * Projectile.Opacity;
	}

	public override bool? CanHitNPC(NPC target)
	{
		return Projectile.penetrate == 2 && !target.friendly;
	}

	public override void AI()
	{
		Projectile.ai[0] += 1f;

		if (Projectile.ai[0] == 1f && Main.netMode != NetmodeID.MultiplayerClient)
		{
			Projectile.ai[1] = Main.rand.NextFloat(-0.01f, 0.01f);
			Projectile.netUpdate = true;
		}

		FadeInAndOut();

		Projectile.velocity *= 0.99f;
		Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[1]);

		if (++Projectile.frameCounter >= 4)
		{
			Projectile.frameCounter = 0;
			
			if (++Projectile.frame >= Main.projFrames[Projectile.type])
			{
				Projectile.frame = 0;
			}
		}

		if (Projectile.ai[0] >= 60f)
		{
			Projectile.Kill();
		}

		Projectile.direction = Projectile.spriteDirection = (Projectile.velocity.X > 0f) ? 1 : -1;
		Projectile.rotation = Projectile.velocity.ToRotation();

		if (Projectile.spriteDirection == -1)
		{
			Projectile.rotation += MathHelper.Pi;
		}
	}

	private void FadeInAndOut()
	{
		if (Projectile.ai[0] <= 50f)
		{
			Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0.8f, 0.03f);
			return;
		}

		Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 0f, 0.25f);
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		Main.player[Projectile.owner].Heal(15);

		if (Projectile.ai[0] < 50)
		{
			Projectile.ai[0] = 50;
		}
	}
}