using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.NPCs.Mapping.Forest.GrovetenderBoss;

internal class TinyBoulder : ModProjectile
{
	private static Asset<Texture2D> Glow = null;

	public override void SetStaticDefaults()
	{
		Main.projFrames[Type] = 3;

		Glow = ModContent.Request<Texture2D>(Texture + "_Glow");
	}

	public override void SetDefaults()
	{
		Projectile.aiStyle = -1;
		Projectile.timeLeft = 20 * 60;
		Projectile.hostile = true;
		Projectile.friendly = false;
		Projectile.Size = new(20);
		Projectile.penetrate = -1;
		Projectile.tileCollide = false;
		Projectile.extraUpdates = 1;
		Projectile.frame = Main.rand.Next(3);
	}

	public override void AI()
	{
		Projectile.rotation += Projectile.velocity.X * 0.02f;
		Projectile.velocity.Y += 0.04f;
		Projectile.tileCollide = Projectile.velocity.Y > 0;

		if (Main.rand.NextBool(20))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CorruptionThorns, Projectile.velocity.X, Projectile.velocity.Y);
		}
	}

	public override void OnKill(int timeLeft)
	{
		for (int i = 0; i < 12; ++i)
		{
			int id = Main.rand.NextBool(3) ? ModContent.DustType<EntDust>() : DustID.CorruptionThorns;
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, id, Projectile.velocity.X, Projectile.velocity.Y);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY);
		float scale = MathF.Sin(Main.GameUpdateCount * 0.06f);
		Color color = Color.White * (scale * 0.4f + 0.6f);
		Rectangle source = new(0, Projectile.frame * 22, 20, 20);

		Main.EntitySpriteDraw(tex, position, source, lightColor, Projectile.rotation, source.Size() / 2f, 1f, SpriteEffects.None, 0);
		Main.EntitySpriteDraw(Glow.Value, position, source, color, Projectile.rotation, source.Size() / 2f, scale * 0.1f + 0.9f, SpriteEffects.None, 0);
		return false;
	}
}