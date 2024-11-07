using PathOfTerraria.Common.Subworlds.BossDomains;
using PathOfTerraria.Common.UI;
using SubworldLibrary;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Utility;

internal class EoWPortal : ModProjectile
{
	private ref float Timer => ref Projectile.ai[0];

	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.timeLeft = 2;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(20, 48);
		Projectile.Opacity = 0.5f;
		Projectile.netImportant = true;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		Projectile.timeLeft++;
		Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, 1f, 0.05f);
		Projectile.velocity *= 0.96f;

		if (Main.rand.NextBool(14))
		{
			Vector2 vel = new Vector2(-Main.rand.NextFloat(4, 8), 0).RotatedBy(Projectile.rotation);
			Dust.NewDustPerfect(Projectile.Center + new Vector2(8, Main.rand.NextFloat(-16, 16)), DustID.PurpleTorch, vel);
		}

		Lighting.AddLight(Projectile.Center, TorchID.Purple);

		foreach (Player player in Main.ActivePlayers)
		{
			if (player.Hitbox.Intersects(Projectile.Hitbox) && Main.myPlayer == player.whoAmI)
			{
				SubworldSystem.Enter<EaterDomain>();
			}
		}
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return Color.White;
	}

	//public override bool PreDraw(ref Color lightColor)
	//{
	//	Texture2D tex = TextureAssets.Projectile[Type].Value;

	//	for (int i = 0; i < 3; ++i)
	//	{
	//		float rotation = Projectile.rotation * (i % 2 == 0 ? -1 : 1);
	//		Vector2 position = Projectile.Center - Main.screenPosition;
	//		Color color = lightColor * ((3 - i) * 0.2f) * Projectile.Opacity;
	//		Main.spriteBatch.Draw(tex, position, null, color, rotation, tex.Size() / 2f, 1f - i * 0.2f, SpriteEffects.None, 0);
	//	}

	//	return false;
	//}
}
