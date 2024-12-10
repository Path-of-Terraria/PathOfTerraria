using PathOfTerraria.Common.World.Generation;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Utility;

internal class WoFRitualProj : ModProjectile
{
	public override string Texture => "Terraria/Images/NPC_0";

	private bool HasInit
	{
		get => Projectile.ai[0] == 1;
		set => Projectile.ai[0] = value ? 1 : 0;
	}

	private ref float Timer => ref Projectile.ai[1];
	private ref float ExplosionTimer => ref Projectile.ai[2];

	private float explosionCap = 15;
	
	public override void SetDefaults()
	{
		Projectile.friendly = false;
		Projectile.hostile = false;
		Projectile.tileCollide = false;
		Projectile.Size = new Vector2(80, 80);
		Projectile.Opacity = 0f;
	}

	public override bool? CanDamage()
	{
		return false;
	}

	public override void AI()
	{
		Projectile.timeLeft++;
		Projectile.rotation += 0.15f;
		
		if (!HasInit)
		{
			HasInit = true;

		}

		Timer++;
		ExplosionTimer++;

		Projectile.Opacity = 1 - explosionCap / 15f;

		if (ExplosionTimer > explosionCap)
		{
			Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(180, 180);
			float size = Main.rand.NextFloat(1, 3);
			Digging.CircleOpening(pos / 16f, size);

			for (int i = 0; i < 8; ++i)
			{
				Gore.NewGore(Projectile.GetSource_FromThis(), pos, Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(2, 5), GoreID.Smoke1 + Main.rand.Next(3));
			}

			for (int i = 0; i < 30; ++i)
			{
				Dust.NewDustPerfect(pos, DustID.Torch, Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(1, 5));
			}

			Vector2 rotation = (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2();
			PunchCameraModifier modifier = new(pos, rotation, 4, 10f, 6, 4000, "WoFRitual");
			Main.instance.CameraModifiers.Add(modifier);

			SoundEngine.PlaySound(SoundID.Item14 with { PitchRange = (-0.8f, 0.2f), Volume = 0.7f }, pos);

			int type = ModContent.ProjectileType<ExplosionHitbox>();
			IEntitySource source = Projectile.GetSource_FromThis();

			Projectile.NewProjectile(source, Projectile.Center, Vector2.Zero, type, 20, 6f, Projectile.owner, 20 * size, 20 * size);

			explosionCap -= 0.5f;
			ExplosionTimer = 0;
		}

		if (explosionCap <= 0)
		{
			Projectile.Kill();

			for (int i = 0; i < 14; ++i)
			{
				Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(3, 6), GoreID.Smoke1 + Main.rand.Next(3));
			}

			for (int i = 0; i < 45; ++i)
			{
				Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(1, 5));
			}

			Vector2 rotation = (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2();
			PunchCameraModifier modifier = new(Projectile.Center, rotation, 9, 10f, 100, 6000, "SkeletronRitual");
			Main.instance.CameraModifiers.Add(modifier);

			IEntitySource src = Projectile.GetSource_Death();
			int type = ModContent.ProjectileType<WoFPortal>();
			Projectile.NewProjectile(src, Projectile.Center, new Vector2(0, -1), type, 0, 0, Main.myPlayer);

			SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.2f, Volume = 1f }, Projectile.Center);
			Digging.CircleOpening(Projectile.Center / 16f, 8);

			type = ModContent.ProjectileType<ExplosionHitbox>();

			Projectile.NewProjectile(src, Projectile.Center, Vector2.Zero, type, 30, 6f, Projectile.owner, 18 * 8, 18 * 8);
		}
	}
	public override bool PreDraw(ref Color lightColor)
	{
		DrawPortal(lightColor);

		return false;
	}

	private void DrawPortal(Color lightColor)
	{
		Main.instance.LoadProjectile(ModContent.ProjectileType<WoFPortal>());
		Texture2D tex = TextureAssets.Projectile[ModContent.ProjectileType<WoFPortal>()].Value;

		for (int i = 0; i < 3; ++i)
		{
			float rotation = Projectile.rotation * (i % 2 == 0 ? -1 : 1);
			Vector2 position = Projectile.Center - Main.screenPosition;
			Color color = lightColor * ((3 - i) * 0.2f) * Projectile.Opacity;
			Main.spriteBatch.Draw(tex, position, null, color, rotation, tex.Size() / 2f, 1f - i * 0.2f, SpriteEffects.None, 0);
		}
	}
}
