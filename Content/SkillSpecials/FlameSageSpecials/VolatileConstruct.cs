using PathOfTerraria.Common;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.SkillPassives.FlameSage;
using PathOfTerraria.Content.Skills.Summon;
using PathOfTerraria.Content.SkillTrees;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;

namespace PathOfTerraria.Content.SkillSpecials.FlameSageSpecials;

public class VolatileConstruct(SkillTree tree) : SkillSpecial(tree)
{
	public const float Seconds = 3;

	public override string DisplayTooltip => string.Format(base.DisplayTooltip, Seconds);

	public class VolatileSentry : FlameSage.FlameSentry
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			NPC.damage = 30;
		}

		public override void OnSyncedSpawn()
		{
			int slowBurn = Owner.GetPassiveStrength<FlameSageTree, SlowBurn>();
			if (slowBurn > 0)
			{
				InitializeTimeLeft((int)(60 * SlowBurn.Seconds));
			}
		}

		public override void AI()
		{
			int slowBurn = Owner.GetPassiveStrength<FlameSageTree, SlowBurn>();
			if (slowBurn > 0)
			{
				Rectangle hitbox = NPC.Hitbox;
				hitbox.Inflate(100, 100);

				int accelerant = Owner.GetPassiveStrength<FlameSageTree, Accelerant>();
				int dotDamage = (int)((NPC.damage * 0.2f * slowBurn) * (1 + (accelerant * Accelerant.DamageIncrease)));

				FlamethrowerNPC.DealDoT(hitbox, dotDamage, Owner, static (npc) =>
				{
					int size = (npc.width * npc.height) / 500;
					for (int i = 0; i < size; i++)
					{
						var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Torch, Scale: Main.rand.NextFloat(1, 3));
						dust.noGravity = true;
						dust.velocity = -Vector2.UnitY;
					}
				});
			}

			if (TimeLeft < 80)
			{
				if (Main.rand.NextBool((TimeLeft / 4) + 1))
				{
					Vector2 position = NPC.Center + Main.rand.NextVector2Unit() * 50;
					Vector2 velocity = position.DirectionTo(NPC.Center) * Main.rand.NextFloat(2, 4);
					Dust.NewDustPerfect(position, DustID.Torch, velocity).noGravity = true;
				}

				Lighting.AddLight(NPC.Center, Color.Red.ToVector3() * (1f - (TimeLeft / 80f)));
			}

			if (NPC.Opacity == 0) //Just spawned in
			{
				SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown, NPC.Center);
			}

			NPC.Opacity = Math.Min(NPC.Opacity + 0.08f, 1);
		}

		public override void OnKill()
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				Projectile.NewProjectile(NPC.GetSource_Death(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<Explosion>(), NPC.damage, 3, Owner.whoAmI);
			}
		}

		public override void UpdateLifeRegen(ref int damage)
		{
			int slowBurn = Owner.GetPassiveStrength<FlameSageTree, SlowBurn>();
			if (slowBurn > 0)
			{
				int accelerant = Owner.GetPassiveStrength<FlameSageTree, Accelerant>();

				float slowBurnDrain = slowBurn * (NPC.lifeMax / SlowBurn.Seconds);
				float accelerantDrain = 1 + (accelerant * Accelerant.LifeLoss);

				NPC.lifeRegen -= (int)(slowBurnDrain * accelerantDrain * 2);
			}
		}
	}

	public class Explosion : ModProjectile
	{
		public const int Duration = 20;
		public float Progress => 1f - (float)Projectile.timeLeft / Duration;
		public override string Texture => "Terraria/Images/Projectile_0";

		public override void SetDefaults()
		{
			Projectile.Size = new(150);
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.timeLeft = Duration;
		}

		public override void AI()
		{
			if (Projectile.localAI[0]++ == 0) //Just spawned
			{
				int blastForce = Main.player[Projectile.owner].GetPassiveStrength<FlameSageTree, BlastForce>();
				if (blastForce > 0)
				{
					Projectile.Size *= (1f + (blastForce * BlastForce.Increase));
				}

				Rectangle size = Projectile.Hitbox;

				for (int i = 0; i < 30; i++)
				{
					Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f) * Main.rand.NextFloat(0.2f, 1f);
					Dust.NewDustPerfect(Projectile.Center, DustID.Torch, vel, 0, default, Main.rand.NextFloat(1.5f, 3f));
				}

				for (int j = 0; j < 5; j++)
				{
					Vector2 vel2 = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(0.2f, 1f);
					Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, vel2, 61 + Main.rand.Next(3));
				}

				SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D circle = TextureAssets.GlowMask[239].Value;
			Texture2D line = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;

			float scale = ((1f / circle.Width) * Projectile.width) * Projectile.scale * Progress;
			float opacity = 1f - Progress;

			Main.EntitySpriteDraw(circle, Projectile.Center - Main.screenPosition, null, Color.OrangeRed with { A = 0 } * opacity, 0, circle.Size() / 2, scale, default);
			Main.EntitySpriteDraw(circle, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 } * opacity, 0, circle.Size() / 2, scale, default);

			Vector2 size = new Vector2(opacity, 3) * ((1f / line.Width) * Projectile.width) * Projectile.scale * Progress;
			float rotation = Projectile.rotation + MathHelper.PiOver2;

			Main.EntitySpriteDraw(line, Projectile.Center - Main.screenPosition, null, Color.OrangeRed with { A = 0 }, rotation, line.Size() / 2, size, default);
			Main.EntitySpriteDraw(line, Projectile.Center - Main.screenPosition, null, Color.White with { A = 0 }, rotation, line.Size() / 2, size * 0.8f, default);

			return false;
		}
	}
}