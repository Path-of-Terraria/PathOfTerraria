using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.PassiveProjectiles;

internal class StarcallerStar : ModProjectile
{
	private static Asset<Texture2D> Back = null;

	private int Target => (int)Projectile.ai[0];
	private ref float Timer => ref Projectile.ai[1];

	public override void SetStaticDefaults()
	{
		Back = ModContent.Request<Texture2D>(Texture + "Back");
	}

	public override void SetDefaults()
	{
		Projectile.Size = new Vector2(24);
		Projectile.friendly = true;
		Projectile.hostile = false;
		Projectile.DamageType = DamageClass.Magic;
		Projectile.timeLeft = 2;
		Projectile.extraUpdates = 1;
		Projectile.tileCollide = false;
	}

	public override void AI()
	{
		Projectile.timeLeft++;

		NPC npc = Main.npc[Target];

		if (!npc.CanBeChasedBy() && npc.type != NPCID.TargetDummy)
		{
			Projectile.Kill();
			return;
		}

		Timer++;
		Projectile.Center = new Vector2(MathHelper.Lerp(Projectile.Center.X, npc.Center.X, 0.15f), Projectile.Center.Y);
		float xOff = (Projectile.Center.X - npc.Center.X) / 50f;
		Projectile.velocity.Y = MathHelper.Lerp(2, 12, 1 - Math.Min(xOff, 1f));
		Projectile.rotation = MathHelper.Clamp(xOff, -1, 1) * 0.6f;

		if (Main.rand.NextBool(9))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BlueFairy);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Vector2 position = Projectile.Center - Main.screenPosition;
		Rectangle src = new(28 * (int)((Timer / 3f) % 2), 0, 26, 36);
		Main.spriteBatch.Draw(Back.Value, position, src, Color.White with { A = 0 }, Projectile.rotation, new Vector2(13, 24), 1f, SpriteEffects.None, 0);
		return true;
	}

	public override void PostDraw(Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 }, Projectile.rotation, tex.Size() / 2f, 1f, SpriteEffects.None, 0);
	}
}
