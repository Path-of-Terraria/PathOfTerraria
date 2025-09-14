using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.NPCs;
using PathOfTerraria.Content.SkillSpecials.FlameSageSpecials;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;

namespace PathOfTerraria.Content.Skills.Summon;

public class FlameSage : SummonSkill
{
	public override int MaxLevel => 3;
	public override int SummonNPCType => Tree.Specialization switch
	{
		Flamethrower => ModContent.NPCType<Flamethrower.FlamethrowerSentry>(),
		MoltenSentinel => ModContent.NPCType<MoltenSentinel.MoltenSentry>(),
		VolatileConstruct => ModContent.NPCType<VolatileConstruct.VolatileSentry>(),
		_ => ModContent.NPCType<FlameSentry>()
	};

	public override void LevelTo(byte level)
	{
		Level = level;
		Cooldown = MaxCooldown = 120;
		ManaCost = 20 - Level * 5;
		Duration = SentryNPC.DefaultSentryDuration;
		WeaponType = ItemType.None;
	}

	public class FlameSentry : SentryNPC
	{
		public ref float Cooldown => ref NPC.ai[0];
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DD2LightningAuraT1;

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			Main.npcFrameCount[Type] = 6;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			NPC.Size = new(20);
			NPC.damage = 20;
			NPC.noGravity = true;
			NPC.HitSound = SoundID.NPCHit15;
			NPC.DeathSound = SoundID.NPCDeath25;
			NPC.Opacity = 0;
		}

		public override void AI()
		{
			const int cooldownMax = 60;
			const int range = 300;

			if ((Cooldown = Math.Max(Cooldown - 1, 0)) == 0)
			{
				Terraria.Utilities.NPCUtils.TargetSearchResults results = FindTarget();
				NPC target = results.NearestNPC;

				if (results.FoundNPC && Collision.CanHit(NPC, target) && target.DistanceSQ(NPC.Center) < range * range)
				{
					int damage = (int)Owner.GetDamage(DamageClass.Summon).ApplyTo(NPC.damage);
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(target.Center) * 10, ModContent.ProjectileType<FlameSentryFireball>(), damage, 3, Owner.whoAmI);

					Cooldown = cooldownMax;
				}
			}

			if (NPC.Opacity == 0) //Just spawned in
			{
				SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown, NPC.Center);
			}

			NPC.Opacity = Math.Min(NPC.Opacity + 0.08f, 1);
		}

		public override void OnKill()
		{
			for (int i = 0; i < 3; i++)
			{
				ParticleOrchestrator.SpawnParticlesDirect(ParticleOrchestraType.WallOfFleshGoatMountFlames, new() { PositionInWorld = NPC.Center });
			}

			for (int i = 0; i < 10; i++)
			{
				var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, Scale: 2);
				dust.velocity = dust.position.DirectionFrom(NPC.Center) * 3;
				dust.fadeIn = 2f;
				dust.noGravity = true;
			}
		}

		public override void FindFrame(int frameHeight)
		{
			NPC.frameCounter = (NPC.frameCounter + 0.2f) % Main.npcFrameCount[Type];
			NPC.frame.Y = frameHeight * (int)NPC.frameCounter;
		}
	}

	public class FlameSentryFireball : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DD2FlameBurstTowerT1Shot;

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.DD2FlameBurstTowerT1Shot);

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		public override void AI()
		{
			if (Projectile.Opacity == 0) //Just spawned in
			{
				SoundEngine.PlaySound(SoundID.DD2_FlameburstTowerShot, Projectile.Center);
			}

			if (Main.rand.NextBool())
			{
				Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch).noGravity = true;
			}

			Projectile.Opacity = Math.Min(Projectile.Opacity + 0.1f, 1);
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (++Projectile.ai[0] > 30)
			{
				Projectile.velocity.Y += 0.1f;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Color color = Projectile.GetAlpha(Color.White) with { A = 0 };
			Vector2 center = Projectile.Center - Main.screenPosition;

			ProjectileID.Sets.TrailCacheLength[Type] = 8;
			ProjectileID.Sets.TrailingMode[Type] = 2;

			for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; i++)
			{
				Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
				Color fadeColor = color * (1f - (i / (float)(ProjectileID.Sets.TrailCacheLength[Type] - 1f)));

				Main.EntitySpriteDraw(texture, trailPos, null, fadeColor * 0.5f, Projectile.oldRot[i], texture.Size() / 2, Projectile.scale, SpriteEffects.None);
			}

			Main.EntitySpriteDraw(texture, center, null, color, Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None);
			Main.EntitySpriteDraw(texture, center, null, color, Projectile.rotation, texture.Size() / 2, Projectile.scale * 0.8f, SpriteEffects.None);

			return false;
		}

		public override void OnKill(int timeLeft)
		{
			const int area = 50;

			Projectile.Resize(area, area);

			for (int i = 0; i < 2; i++)
			{
				ParticleOrchestrator.SpawnParticlesDirect(ParticleOrchestraType.AshTreeShake, new() { PositionInWorld = Projectile.Center });
			}

			for (int i = 0; i < 4; i++)
			{
				var dust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Scale: 2);
				dust.velocity = Main.rand.NextVector2Unit() * 0.5f;
				dust.fadeIn = 2f;
				dust.noGravity = true;
			}

			for (int i = 0; i < 10; i++)
			{
				var dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);
				dust.velocity = dust.position.DirectionFrom(Projectile.Center) * Main.rand.NextFloat(0.5f, 1);
				dust.fadeIn = 1.1f;
				dust.noGravity = true;
			}

			Projectile.Damage();
			SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact, Projectile.Center);
		}
	}
}