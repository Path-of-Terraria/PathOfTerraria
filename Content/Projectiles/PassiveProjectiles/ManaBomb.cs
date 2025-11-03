using Terraria.GameContent;

namespace PathOfTerraria.Content.Projectiles.PassiveProjectiles;

internal class ManaBomb : ModProjectile
{
	private ref float Timer => ref Projectile.localAI[0];

	private int Target => (int)Projectile.ai[0];
	private Vector2 TargetPosition => new(Projectile.ai[1], Projectile.ai[2]);

	private Vector2 RealTarget => Target == -1 ? TargetPosition : Main.npc[Target].Center;

	public override void SetDefaults()
	{
		Projectile.Size = new Vector2(22);
		Projectile.friendly = true;
		Projectile.DamageType = DamageClass.Magic;
		Projectile.timeLeft = 2;
	}

	public override void AI()
	{
		Timer++;
		Projectile.timeLeft++;

		if (Projectile.DistanceSQ(RealTarget) < 20 * 20)
		{
			Projectile.Kill();
		}

		Projectile.velocity = Vector2.Zero;
		Vector2 offsetTarget = RealTarget + Projectile.DirectionTo(RealTarget).RotatedBy(MathF.Sin(Projectile.Distance(RealTarget) / 80f) * MathHelper.PiOver2) * 120;
		Vector2 oldCenter = Projectile.Center;
		Projectile.Center = Vector2.Lerp(Projectile.Center, offsetTarget, 0.05f);
		Projectile.rotation = (Projectile.Center - oldCenter).ToRotation() + MathHelper.PiOver2;
	}

	public override void OnKill(int timeLeft)
	{
		int type = ModContent.ProjectileType<ManaBombExplosion>();
		Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Projectile.velocity, type, Projectile.damage, 8f, Projectile.owner, 54, 54, Projectile.rotation);
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		Vector2 pos = Projectile.Center - Main.screenPosition;
		float glowStrength = Utils.GetLerpValue(0, 1f, RealTarget.Distance(Projectile.Center) / 120f, true);
		var glowSrc = new Rectangle(0, 24, 26, 26);
		var src = new Rectangle(2, 0, 22, 22);
		var mainColor = Color.Lerp(lightColor, Color.White, 0.4f * GetSine(MathHelper.PiOver4 * 3, (0.4f, 0.8f)));
		Color glow = Color.White with { A = 0 } * 0.6f * glowStrength;

		Main.spriteBatch.Draw(tex, pos, src, mainColor, Projectile.rotation, src.Size() / 2f, Projectile.scale * GetSine(0), SpriteEffects.None, 0);
		Main.spriteBatch.Draw(tex, pos, glowSrc, glow, Projectile.rotation, glowSrc.Size() / 2f, Projectile.scale * GetSine(MathHelper.PiOver2), SpriteEffects.None, 0);
		Main.spriteBatch.Draw(tex, pos, glowSrc, glow, Projectile.rotation, glowSrc.Size() / 2f, Projectile.scale * GetSine(MathHelper.PiOver4) * 0.5f, SpriteEffects.None, 0);

		return false;

		float GetSine(float off, (float scale, float add)? offsets = null)
		{
			offsets ??= (0.1f, 1f);
			return MathF.Sin(Timer * 0.07f + off) * offsets.Value.scale + offsets.Value.add;
		}
	}
}
