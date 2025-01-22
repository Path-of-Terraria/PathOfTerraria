using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

internal class GroveBoulder : ModProjectile
{
	public const float Gravity = 0.2f;

	private static Asset<Texture2D> Glow = null;

	internal bool Controlled
	{
		get => Projectile.ai[0] == 1;
		set => Projectile.ai[0] = value ? 1 : 0;
	}

	public override void SetStaticDefaults()
	{
		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");
	}

	public override void SetDefaults()
	{
		Projectile.aiStyle = -1;
		Projectile.timeLeft = 20 * 60;
		Projectile.hostile = true;
		Projectile.friendly = false;
		Projectile.Size = new(44);
		Projectile.penetrate = -1;
		Projectile.extraUpdates = 1;
	}

	public override void AI()
	{
		Projectile.tileCollide = !Controlled;

		if (!Controlled)
		{
			Projectile.rotation += Projectile.velocity.X * 0.02f;
			Projectile.velocity.Y += Gravity / (Projectile.extraUpdates + 1f);

			Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
		}
		else
		{
			Projectile.velocity = Vector2.Zero;
		}

		if (Main.rand.NextBool(20))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, Projectile.velocity.X, Projectile.velocity.Y);
		}

		Controlled = false;
	}

	public override void OnKill(int timeLeft)
	{
		for (int i = 0; i < 12; ++i)
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Stone, Projectile.velocity.X, Projectile.velocity.Y);
		}
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
		{
			Projectile.velocity.X = -oldVelocity.X * 0.95f;

			Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.8f }, Projectile.position);
		}

		if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
		{
			Projectile.velocity.Y = -oldVelocity.Y * 0.3f;
		}

		return false;
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY);
		float scale = MathF.Sin(Main.GameUpdateCount * 0.06f);
		Color color = Color.White * (scale * 0.4f + 0.6f);

		Main.EntitySpriteDraw(Glow.Value, position, null, color, Projectile.rotation, Glow.Size() / 2f, scale * 0.1f + 0.9f, SpriteEffects.None, 0);
		Main.EntitySpriteDraw(tex, position, null, lightColor, Projectile.rotation, tex.Size() / 2f, 1f, SpriteEffects.None, 0);
		return false;
	}
}