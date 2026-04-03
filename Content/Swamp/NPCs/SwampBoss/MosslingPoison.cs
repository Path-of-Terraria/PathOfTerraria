using PathOfTerraria.Content.Dusts;
using System;
using System.Collections.Generic;
using Terraria.ID;

namespace PathOfTerraria.Content.Swamp.NPCs.SwampBoss;

internal class MosslingPoison : ModProjectile
{
	private ref float Timer => ref Projectile.ai[0];
	private ref float GlowStrength => ref Projectile.ai[1];

	public override void SetDefaults()
	{
		Projectile.CloneDefaults(ProjectileID.Bullet);
		Projectile.aiStyle = -1;
		Projectile.Size = new Vector2(18);
		Projectile.timeLeft = 600;
		Projectile.Opacity = 1f;
		Projectile.friendly = false;
		Projectile.hostile = true;
		Projectile.light = 0;
		Projectile.hide = true;
	}

	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
	{
		overWiresUI.Add(index);
	}

	public override void AI()
	{
		Timer++;
		GlowStrength = MathHelper.Lerp(GlowStrength, Projectile.wet ? 0.7f : 0.1f, 0.05f);

		Projectile.rotation += Projectile.velocity.X * 0.04f;

		if (Projectile.timeLeft < 30f)
		{
			Projectile.Opacity = Projectile.timeLeft / 30f;
		}

		if (Timer > 360)
		{
			Projectile.velocity.X *= 0.97f;
			Projectile.velocity.Y += 0.02f;

			if (Projectile.velocity.Y > 8)
			{
				Projectile.velocity.Y = 8;
			}
		}

		if (Main.rand.NextBool(14))
		{
			int type = Main.rand.NextBool(3) ? ModContent.DustType<BrightVenomDust>() : DustID.Venom;
			int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, type, Projectile.velocity.X, Projectile.velocity.Y, Scale: Main.rand.NextFloat(2, 3));
			Main.dust[dust].noGravity = true;
		}
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return  Color.Lerp(Lighting.GetColor(Projectile.Center.ToTileCoordinates()), Color.White, GlowStrength) * Projectile.Opacity;
	}
}
