using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace PathOfTerraria.Content.Projectiles.Ranged.Javelin;

internal class RottenboneThrown() : JavelinThrown("RottenboneThrown", new(116), DustID.CorruptGibs)
{
	public override void AI()
	{
		Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4 - 0.05f;
		Projectile.velocity.Y -= 0.05f;

		if (Main.rand.NextBool(6))
		{
			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.CorruptGibs, Scale: Main.rand.NextFloat(1.2f, 1.8f));
		}

		if (UsingAlt && Projectile.localAI[0]++ > 60)
		{
			Vector2 location = Projectile.Center;
			Vector2 tip = ItemSize.RotatedBy(Projectile.rotation + MathHelper.PiOver2);

			for (int i = 0; i < 8; ++i)
			{
				IEntitySource source = Projectile.GetSource_Death();
				int type = ModContent.ProjectileType<RottenboneChunks>();
				var position = Vector2.Lerp(location, location + tip, i / 7f);

				int isChunk = i switch
				{
					7 or 6 or 5 or 3 or 0 => 0,
					_ => 1
				};

				int proj = Projectile.NewProjectile(source, position, Projectile.velocity, type, Projectile.damage / 2, Projectile.knockBack, Projectile.owner, isChunk);

				Main.projectile[proj].scale = i switch
				{
					7 or 6 => 0.8f,
					5 or 4 or 3 => 0.5f,
					_ => 1.1f
				};
			}

			Projectile.Kill();
		}
	}

	public override bool? CanDamage()
	{
		return true;
	}

	internal class RottenboneChunks : ModProjectile
	{
		public override string Texture => $"{PoTMod.ModName}/Assets/Items/Gear/Weapons/Javelins/{GetType().Name}";

		private bool IsChunk => Projectile.ai[0] == 1;

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 6;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
			Projectile.timeLeft = 6000;
			Projectile.Size = new Vector2(18);
			Projectile.penetrate = 3;
			Projectile.extraUpdates = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void AI()
		{
			if (Projectile.frameCounter == 0)
			{
				Projectile.frame = IsChunk ? Main.rand.Next(3) + 3 : Main.rand.Next(3);
				Projectile.frameCounter = 1;
			}

			if (Main.rand.NextBool(20))
			{
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, IsChunk ? DustID.CorruptGibs : DustID.Bone);
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.penetrate--;

			if (Projectile.penetrate <= 0)
			{
				Projectile.Kill();
			}
			else
			{
				Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
				SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

				if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
				{
					Projectile.velocity.X = -oldVelocity.X;
				}

				if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
				{
					Projectile.velocity.Y = -oldVelocity.Y * 0.5f;
				}
			}

			return false;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (!IsChunk)
			{
				modifiers.FinalDamage *= 1.2f;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (IsChunk)
			{
				target.AddBuff(BuffID.Poisoned, 3 * 60);
			}

			Projectile.Kill();
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 8; ++i)
			{
				short id = IsChunk ? DustID.CorruptGibs : DustID.Bone;
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, id, Projectile.velocity.X, Projectile.velocity.Y);
			}
		}
	}
}