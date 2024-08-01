using PathOfTerraria.Content.Items.Gear.Weapons.Javelins;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Ranged.Javelin;

internal class MoltenDangpaThrown() : JavelinThrown("MoltenDangpaThrown", new(94), DustID.Torch)
{
	public override void AI()
	{
		if (Projectile.timeLeft > 3)
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
			Projectile.velocity.Y -= 0.05f;
		}

		if (UsingAlt && Main.rand.NextBool(6))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);
		}
	}

	public override bool? CanDamage()
	{
		return true;
	}

	public override bool OnTileCollide(Vector2 oldVelocity)
	{
		CheckExplode();
		return !UsingAlt;
	}

	public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
	{
		CheckExplode();

		if (UsingAlt)
		{
			target.AddBuff(BuffID.OnFire, 180);
		}
	}

	public override void OnHitPlayer(Player target, Player.HurtInfo info)
	{
		CheckExplode();

		if (UsingAlt)
		{
			target.AddBuff(BuffID.OnFire, 180);
		}
	}

	private void CheckExplode()
	{
		if (Projectile.timeLeft > 3 && UsingAlt)
		{
			OnKill(Projectile.timeLeft);

			for (int i = 0; i < 40; ++i)
			{
				Vector2 velocity = Main.rand.NextVector2Circular(12, 12);
				Dust.NewDustPerfect(Projectile.Center, DustID.Torch, velocity, Scale: Main.rand.NextFloat(1, 2));
			}

			Projectile.timeLeft = 3;
			Projectile.Resize(180, 180);
			Projectile.velocity = Vector2.Zero;
			Projectile.hide = true;
		}
	}

	public override void OnKill(int timeLeft)
	{
		if (Projectile.timeLeft > 3)
		{
			DeathDust();

			if (UsingAlt && Main.myPlayer == Projectile.owner)
			{
				int projCount = Main.rand.Next(3, 6);

				for (int i = 0; i < projCount; ++i)
				{
					int type = ModContent.ProjectileType<MoltenDangpaBubbles>();
					Vector2 velocity = -Projectile.velocity.RotatedByRandom(1f) * Main.rand.NextFloat(0.5f, 1f);
					Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center - Projectile.velocity * 3, velocity, type, (int)(Projectile.damage * 0.75f), 0);
				}
			}
		}
	}

	private void DeathDust()
	{
		Vector2 location = Projectile.Center;
		Vector2 tip = ItemSize.RotatedBy(Projectile.rotation + MathHelper.PiOver2);

		for (int i = 0; i < 16; ++i)
		{
			Dust.NewDust(location + tip * Main.rand.NextFloat(), 1, 1, UsingAlt ? DustType : DustID.Lead);
		}
	}

	public override bool PreDraw(ref Color lightColor)
	{
		int moltenId = ModContent.ItemType<MoltenDangpa>();
		Main.instance.LoadItem(moltenId);
		Texture2D tex = UsingAlt ? TextureAssets.Projectile[Type].Value : TextureAssets.Item[moltenId].Value;
		Color color = lightColor * Projectile.Opacity;
		Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, tex.Size() * new Vector2(0.75f, 0.25f), 1f, SpriteEffects.None, 0);

		return false;
	}

	public class MoltenDangpaBubbles : ModProjectile
	{
		public override string Texture => $"{PoTMod.ModName}/Assets/Items/Gear/Weapons/Javelins/{GetType().Name}";

		public bool Stuck
		{
			get => Projectile.ai[2] == 1;
			set => Projectile.ai[2] = value ? 1 : 0;
		}

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 3;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.SpikyBall);
			Projectile.timeLeft = 500;
			Projectile.Size = new Vector2(18);
			Projectile.penetrate = -1;
		}

		public override bool PreAI()
		{
			Projectile.frameCounter++;
			Projectile.frame = Projectile.frameCounter / 5 % 3;

			if (Projectile.timeLeft < 300)
			{
				Projectile.Opacity = Projectile.timeLeft / 300f;
			}

			if (Main.rand.NextBool(6))
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);
			}

			return !Stuck;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Stuck = true;
			Projectile.velocity = Vector2.Zero;
			return false;
		}
	}
}