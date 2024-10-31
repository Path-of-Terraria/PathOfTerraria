namespace PathOfTerraria.Content.Projectiles.Utility;

public class LifeStealProjectile : ModProjectile
{
	public override void SetStaticDefaults()
	{
		Main.projFrames[Projectile.type] = 4;
	}

	public override void SetDefaults()
	{
		Projectile.width = 40;
		Projectile.height = 40;

		Projectile.friendly = true;
		Projectile.DamageType = DamageClass.Melee;
		Projectile.ignoreWater = true;
		Projectile.tileCollide = true;

		Projectile.alpha = 255;
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return new Color(255, 255, 255, 0) * Projectile.Opacity;
	}

	public override void AI()
	{
		Projectile.ai[0] += 1f;

		FadeInAndOut();

		Projectile.velocity *= 0.98f;

		if (++Projectile.frameCounter >= 5)
		{
			Projectile.frameCounter = 0;
			;
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
			Projectile.alpha -= 25;
			if (Projectile.alpha < 100)
			{
				Projectile.alpha = 100;
			}

			return;
		}

		Projectile.alpha += 25;
		if (Projectile.alpha > 255)
		{
			Projectile.alpha = 255;
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		SpriteEffects spriteEffects = SpriteEffects.None;
		if (Projectile.spriteDirection == -1)
		{
			spriteEffects = SpriteEffects.FlipHorizontally;
		}

		Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

		int frameHeight = texture.Height / Main.projFrames[Projectile.type];
		int startY = frameHeight * Projectile.frame;

		var sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);

		Vector2 origin = sourceRectangle.Size() / 2f;

		const float offsetX = 20f;
		origin.X = Projectile.spriteDirection == 1 ? sourceRectangle.Width - offsetX : offsetX;

		Color drawColor = Projectile.GetAlpha(lightColor);
		Main.EntitySpriteDraw(texture,
			Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
			sourceRectangle, drawColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

		return false;
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		Player player = Main.LocalPlayer;
		player.Heal(15);
	}
}