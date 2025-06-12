using PathOfTerraria.Common;
using Terraria.GameContent.Drawing;

namespace PathOfTerraria.Content.NPCs.BossDomain.EoLDomain;

internal class PrismatismAfterimage : ModProjectile
{
	private bool Fade
	{
		get => Projectile.ai[0] == 1f;
		set => Projectile.ai[0] = value ? 1f : 0;
	}

	private ref float RainbowProgress => ref Projectile.ai[1];

	private bool Init
	{
		get => Projectile.ai[2] == 1f;
		set => Projectile.ai[2] = value ? 1f : 0;
	}

	public override void SetDefaults()
	{
		Projectile.Size = new Vector2(26);
		Projectile.friendly = false;
		Projectile.hostile = true;
		Projectile.timeLeft = 70000;
		Projectile.Opacity = 0f;
		Projectile.scale = 0f;
		Projectile.tileCollide = false;
		Projectile.aiStyle = -1;
	}

	public override bool CanHitPlayer(Player target)
	{
		return !Fade;
	}

	public override void AI()
	{
		if (!Init)
		{
			RainbowProgress = Main.rand.NextFloat();
			Init = true;
		}

		Projectile.Size = new Vector2(26, 26) * Projectile.scale;
		Projectile.Opacity = MathHelper.Lerp(Projectile.Opacity, !Fade ? 0.5f : 0, 0.05f);
		Projectile.scale = MathHelper.Lerp(Projectile.scale, !Fade ? 1 : 0, 0.05f);
		Projectile.velocity.Y += 0.06f;
		RainbowProgress = (RainbowProgress + 0.005f) % 1;

		if (Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height))
		{
			var settings = new ParticleOrchestraSettings
			{
				PositionInWorld = Projectile.Center,
				MovementVector = Projectile.velocity + new Vector2(0, -38).RotatedByRandom(0.2f) * Main.rand.NextFloat(0.6f, 1.2f),
			};

			ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.RainbowRodHit, settings);

			Projectile.Kill();
		}

		if (Fade)
		{
			Projectile.velocity.Y -= 0.2f;

			if (Projectile.Opacity <= 0.05f)
			{
				Projectile.Kill();
			}
		}

		if (WorldGen.genRand.NextBool(140))
		{
			Prismatism.SpawnDust(Projectile, 0.4f);
		}
	}

	public override Color? GetAlpha(Color lightColor)
	{
		return MathTools.GetRainbowColor(RainbowProgress, 0.8f) * Projectile.Opacity;
	}
}
