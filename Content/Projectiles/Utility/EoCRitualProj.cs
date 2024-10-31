using ReLogic.Content;
using ReLogic.Utilities;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Utility;

internal class EoCRitualProj : ModProjectile
{
	private static Asset<Texture2D> StarTex;

	public override string Texture => "Terraria/Images/NPC_0";

	private bool HasInit
	{
		get => Projectile.ai[0] == 1;
		set => Projectile.ai[0] = value ? 1 : 0;
	}

	private ref float Timer => ref Projectile.ai[1];
	
	private readonly List<StarParticle> _particles = [];
	private SlotId RiserSlot = default;

	public override void SetStaticDefaults()
	{
		StarTex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/Projectiles/Utility/Star");
	}

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

			RiserSlot = SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/StarRiser") { IsLooped = true }, Projectile.Center);

			for (int i = 0; i < 80; ++i)
			{
				var color = Color.Lerp(Color.White, Color.Yellow, Main.rand.NextFloat());
				
				_particles.Add(new StarParticle()
				{
					Position = Projectile.Center - new Vector2(0, Main.rand.NextFloat(700, 1200)).RotatedByRandom(MathHelper.Pi / 3f),
					Velocity = Vector2.Zero,
					Opacity = 0,
					OriginalColor = color,
					MaxOpacity = Main.rand.NextFloat(0.5f, 1),
					Color = color,
					Frame = Main.rand.Next(5),
					TimerOffset = Main.rand.Next(-30, 60),
					ScaleMod = Main.rand.NextFloat(0.5f, 1f)
				});
			}
		}

		UpdateRiser();

		Timer++;

		if (_particles.Count == 0)
		{
			Projectile.Kill();

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				IEntitySource src = Projectile.GetSource_Death();
				int type = ModContent.ProjectileType<EyePortal>();
				Projectile.NewProjectile(src, Projectile.Center, new Vector2(0, -4), type, 0, 0, Main.myPlayer);

				SoundEngine.PlaySound(new SoundStyle($"{PoTMod.ModName}/Assets/Sounds/PortalAppear"), Projectile.Center);
			}

			return;
		}

		const float WaitTime = 180f;

		for (int i = 0; i < _particles.Count; i++)
		{
			StarParticle particle = _particles[i];

			if (Timer < WaitTime + particle.TimerOffset)
			{
				particle.Velocity = new Vector2(0, Timer / 45f).RotatedBy(Main.GlobalTimeWrappedHourly * 0.5f + i);
				particle.Opacity = Timer / WaitTime;
			}
			else
			{
				float adjFactor = (Timer - WaitTime) / 300f;
				particle.Velocity = Vector2.Lerp(particle.Velocity, particle.Position.DirectionTo(Projectile.Center) * 20, 0.015f);

				float dist = particle.Position.Distance(Projectile.Center);

				if (dist < 200f)
				{
					particle.Color = Color.Lerp(Color.Red, particle.OriginalColor, dist / 200f);
					particle.Opacity = MathHelper.Clamp(dist / 100f, 0, 1);
				}

				if (dist < 40)
				{
					particle.Opacity = 0;
					particle.Velocity *= 0.95f;
				}
			}

			particle.Position += particle.Velocity;

			if (Main.rand.NextBool(130))
			{
				Dust.NewDustPerfect(particle.Position, DustID.YellowStarDust, particle.Velocity.RotatedBy(0.3f) * Main.rand.NextFloat(0.8f, 1f));
			}
		}

		if (Timer > 120)
		{
			_particles.RemoveAll(x => x.Opacity == 0);
		}
	}

	private void UpdateRiser()
	{
		if (SoundEngine.TryGetActiveSound(RiserSlot, out ActiveSound sound))
		{
			sound.Pitch = 1 - _particles.Count / 80f * 2;
			sound.Volume = _particles.Count / 80f * 0.75f + 0.25f;

			if (Timer <= 30)
			{
				sound.Volume = Timer / 30f;
			}

			if (_particles.Count == 0)
			{
				sound.Stop();
			}
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		foreach (StarParticle particle in _particles)
		{
			particle.Draw();
		}

		Main.instance.LoadProjectile(ModContent.ProjectileType<EyePortal>());
		Texture2D tex = TextureAssets.Projectile[ModContent.ProjectileType<EyePortal>()].Value;
		float mod = 1 - _particles.Count / 80f;

		for (int i = 0; i < 3; ++i)
		{
			float rotation = Projectile.rotation * (i % 2 == 0 ? -1 : 1);
			Vector2 position = Projectile.Center - Main.screenPosition;
			Color color = lightColor * ((3 - i) * 0.2f) * mod * 0.5f;
			float scale = 1f - i * 0.2f;

			Main.spriteBatch.Draw(tex, position, null, color, rotation, tex.Size() / 2f, scale * mod, SpriteEffects.None, 0);
		}

		return false;
	}

	public class StarParticle
	{
		public Vector2 Position;
		public Vector2 Velocity;
		public Color OriginalColor;
		public Color Color;
		public float Opacity;
		public float MaxOpacity;
		public int Frame;
		public int TimerOffset;
		public float ScaleMod;

		public void Draw()
		{
			var frame = new Rectangle(0, Frame * 24, 22, 22);
			float rotation = Velocity.ToRotation();
			Vector2 scale = new Vector2(MathHelper.Lerp(Velocity.Length() / 2, 1, 0.3f), 1f) * ScaleMod;
			Color color = Color * Opacity * MaxOpacity;
			Vector2 pos = Position - Main.screenPosition;

			Main.spriteBatch.Draw(StarTex.Value, pos, frame, color, rotation, frame.Size() / 2f, scale, SpriteEffects.None, 0);
		}
	} 
}
