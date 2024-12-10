using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Utility;

internal class SkeletronRitualProj : ModProjectile
{
	public override string Texture => "Terraria/Images/NPC_0";

	private bool HasInit
	{
		get => Projectile.ai[0] == 1;
		set => Projectile.ai[0] = value ? 1 : 0;
	}

	private readonly List<SkullParticle> _particles = [];
	private ref float Timer => ref Projectile.ai[1];

	Point16 anchor = default;

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
		const float MaxTimer = 240;

		Projectile.timeLeft++;
		Projectile.rotation += 0.15f;

		Timer++;
		
		if (!HasInit)
		{
			HasInit = true;

			anchor = Projectile.Center.ToTileCoordinates16();
		}

		for (int i = 0; i < _particles.Count; i++)
		{
			SkullParticle particle = _particles[i];

			particle.Velocity = particle.Velocity.RotatedByRandom(0.07f);
			particle.Position += particle.Velocity;
		}

		_particles.RemoveAll(x => x.Opacity <= 0);

		if (Timer > MaxTimer)
		{
			if (_particles.Count == 0)
			{
				Projectile.Kill();
			}

			return;
		}

		if (Timer % 5 == 0)
		{
			Vector2 pos = anchor.ToWorldCoordinates();
			Collision.HitTiles(pos, new Vector2(0, -8), 16, 16);
		}

		if (Timer > 60 && Timer % 15 == 0 || Timer > MaxTimer - 16 && Timer % 2 == 0 && Main.netMode != NetmodeID.Server)
		{
			Vector2 rotation = (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2();
			int time = (int)(Timer / MaxTimer * 12f);
			PunchCameraModifier modifier = new(Projectile.Center, rotation, Timer / MaxTimer * 6f, 10f, time, 4000, "SkeletronRitual");
			Main.instance.CameraModifiers.Add(modifier);

			SpawnSkull();
			BloodDust(15);
		}

		if (Timer == MaxTimer)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				WorldGen.KillTile(anchor.X - 1, anchor.Y, false, false, true);
				WorldGen.KillTile(anchor.X, anchor.Y, false, false, true);
				WorldGen.KillTile(anchor.X + 1, anchor.Y, false, false, true);
				WorldGen.KillTile(anchor.X, anchor.Y + 1, false, false, true);

				if (Main.netMode == NetmodeID.Server)
				{
					NetMessage.SendTileSquare(-1, anchor.X, anchor.Y, 3);
				}
			}

			BloodDust(50);

			Projectile.NewProjectileDirect(Projectile.GetSource_Death(), Projectile.Center, new Vector2(0, -3), ModContent.ProjectileType<SkeletronPortal>(), 0, 0, Main.myPlayer);

			Vector2 rotation = (Main.rand.NextFloat() * ((float)Math.PI * 2f)).ToRotationVector2();
			PunchCameraModifier modifier = new(Projectile.Center, rotation, 9, 10f, 60, 4000, "SkeletronRitual");
			Main.instance.CameraModifiers.Add(modifier);
		}
	}

	private void SpawnSkull()
	{
		var color = Color.Lerp(Color.White, Color.Red, Main.rand.NextFloat());

		_particles.Add(new SkullParticle()
		{
			Position = Projectile.Center,
			Velocity = new Vector2(0, -Main.rand.NextFloat(9, 12)).RotatedByRandom(MathHelper.PiOver2),
			Opacity = Main.rand.NextFloat(0.8f, 1f),
			Color = color,
			Frame = Main.rand.Next(5),
			Effect = Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipVertically
		});
	}

	private void BloodDust(int count)
	{
		Vector2 basePos = anchor.ToWorldCoordinates();

		for (int i = 0; i < count; ++i)
		{
			Vector2 vel = new(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-12f, -6f));
			Dust.NewDust(basePos, 1, 1, DustID.Blood, vel.X, vel.Y, Scale: Main.rand.NextFloat(1, 2.5f));
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		foreach (SkullParticle particle in _particles)
		{
			particle.Draw();
		}

		return false;
	}

	public class SkullParticle
	{
		public Vector2 Position;
		public Vector2 Velocity;
		public Color Color;
		public float Opacity;
		public int Frame;
		public SpriteEffects Effect;

		public void Draw()
		{
			Main.instance.LoadNPC(NPCID.CursedSkull);
			Texture2D tex = TextureAssets.Npc[NPCID.CursedSkull].Value;

			Opacity -= 0.02f;

			var frame = new Rectangle(0, Frame * 30, 26, 28);
			float rotation = Velocity.ToRotation();
			Color color = Color * Opacity;
			Vector2 pos = Position - Main.screenPosition;

			Main.spriteBatch.Draw(tex, pos, frame, color, rotation, frame.Size() / 2f, Vector2.One, Effect, 0);
		}
	}
}
