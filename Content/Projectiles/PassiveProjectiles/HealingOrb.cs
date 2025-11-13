using Terraria.GameContent;

namespace PathOfTerraria.Content.Projectiles.PassiveProjectiles;

internal class HealingOrb : ModProjectile
{
	private float HealPercent => Projectile.ai[0];
	private ref float Timer => ref Projectile.ai[1];

	public override void SetDefaults()
	{
		Projectile.Size = new Vector2(22);
		Projectile.friendly = true;
		Projectile.DamageType = DamageClass.Magic;
		Projectile.timeLeft = 2;
		Projectile.tileCollide = true;
		Projectile.netImportant = true;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		if (Projectile.velocity.LengthSquared() > 0.5f)
		{
			Projectile.velocity *= 0.98f;
		}
		else
		{
			Projectile.velocity = Projectile.velocity.RotatedBy(MathF.Sin(Timer * 0.12f) * 0.06f);
		}

		Projectile.timeLeft++;
		Timer++;

		Projectile.Opacity = 1 - Timer / 600f;

		if (Timer > 600f)
		{
			Projectile.Kill();
		}

		foreach (Player player in Main.ActivePlayers)
		{
			if (player.DistanceSQ(Projectile.Center) < 16 * 16)
			{
				player.Heal((int)(player.statLifeMax2 * HealPercent));
				Projectile.Kill();
			}
			else if (player.DistanceSQ(Projectile.Center) < 100 * 100)
			{
				Projectile.velocity += Projectile.DirectionTo(player.Center) * 0.2f;

				if (Projectile.velocity.LengthSquared() > 8 * 8)
				{
					Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 8;
				}	
			}
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		Vector2 pos = Projectile.Center - Main.screenPosition;
		var glowSrc = new Rectangle(0, 0, 38, 38);
		var src = new Rectangle(0, 40, 38, 38);
		var mainColor = Color.Lerp(lightColor, Color.White, 0.4f);
		Color glow = Color.White with { A = 0 } * 0.6f * Projectile.Opacity;
		float rot = Projectile.rotation;

		Main.spriteBatch.Draw(tex, pos, glowSrc, glow, rot + Main.GameUpdateCount * 0.03f, glowSrc.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
		Main.spriteBatch.Draw(tex, pos, glowSrc, glow * 0.2f, rot - Main.GameUpdateCount * 0.08f, glowSrc.Size() / 2f, Projectile.scale * 1.25f, SpriteEffects.None, 0);
		Main.spriteBatch.Draw(tex, pos, src, mainColor * Projectile.Opacity, rot, src.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

		return false;
	}
}
