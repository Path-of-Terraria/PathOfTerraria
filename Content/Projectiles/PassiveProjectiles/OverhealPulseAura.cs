using Terraria.GameContent;

namespace PathOfTerraria.Content.Projectiles.PassiveProjectiles;

internal class OverhealPulseAura : ModProjectile
{
	public override void SetDefaults()
	{
		Projectile.timeLeft = 2;
		Projectile.Opacity = 1f;
		Projectile.Size = new Vector2(180);
		Projectile.scale = 0.02f;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = 30;
		Projectile.tileCollide = false;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		const float Radius = 360;

		Projectile.timeLeft++;
		Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f, 0.09f);
		Projectile.Opacity = 1 - Projectile.scale;
		Projectile.rotation = MathHelper.Lerp(Projectile.rotation, MathHelper.TwoPi, 0.01f);

		int radius = (int)(Projectile.scale * Radius);
		Projectile.Resize(radius, radius);

		if (Projectile.scale >= 0.99f)
		{
			Projectile.Kill();
		}

		foreach (NPC npc in Main.ActiveNPCs)
		{
			if (npc.CanBeChasedBy() && npc.DistanceSQ(Projectile.Center) < radius * radius && Projectile.localNPCImmunity[npc.whoAmI] <= 0)
			{
				npc.SimpleStrikeNPC(Projectile.damage, Math.Sign(Projectile.Center.X - npc.Center.X));
				Projectile.localNPCImmunity[npc.whoAmI] = Projectile.localNPCHitCooldown;
			}
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		Texture2D tex = TextureAssets.Projectile[Type].Value;
		lightColor = Color.Lerp(lightColor, Color.White, 0.33f) * Projectile.Opacity;
		Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
		return false;
	}
}
