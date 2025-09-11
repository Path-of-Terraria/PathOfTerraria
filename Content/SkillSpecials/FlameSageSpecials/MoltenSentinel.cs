using PathOfTerraria.Common;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using PathOfTerraria.Content.SkillPassives.FlameSage;
using PathOfTerraria.Content.Skills.Summon;
using PathOfTerraria.Content.SkillTrees;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Content.SkillSpecials.FlameSageSpecials;

public class MoltenSentinel(SkillTree tree) : SkillSpecial(tree)
{
	public class MoltenSentry : FlameSage.FlameSentry
	{
		public override void OnSyncedSpawn()
		{
			int furnaceStrength = Owner.GetPassiveStrength<FlameSageTree, LivingFurnace>();
			if (furnaceStrength > 0)
			{
				Aggro += 10 * furnaceStrength;
				NPC.life = NPC.lifeMax = (int)(NPC.lifeMax * (1f + (0.25f * furnaceStrength)));
			}

			int damage = (int)Owner.GetDamage(DamageClass.Summon).ApplyTo(NPC.damage);
			Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MoltenSentryAura>(), damage, 0, Owner.whoAmI, NPC.whoAmI);
		}

		public override void AI()
		{
			if (NPC.Opacity == 0) //Just spawned in
			{
				SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown, NPC.Center);
			}

			NPC.Opacity = Math.Min(NPC.Opacity + 0.08f, 1);
		}
	}

	public class MoltenSentryAura : SkillProjectile<FlameSage>
	{
		public int Parent
		{
			get => (int)Projectile.ai[0];
			set => Projectile.ai[0] = value;
		}

		public override string Texture => "Terraria/Images/FlameRing";

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 3;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.DD2LightningAuraT1);
			Projectile.aiStyle = -1;
			Projectile.Size = new(200);
			Projectile.hide = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 30;
		}

		public override void AI()
		{
			if (Main.npc[Parent] is NPC parent && parent.active)
			{
				Projectile.Center = Main.npc[Parent].Center;
			}
			else
			{
				Projectile.Kill();
			}

			for (int i = 0; i < 2; i++)
			{
				Vector2 position = Projectile.Center + Main.rand.NextVector2Unit() * (Projectile.width / 2);
				if (!Collision.SolidCollision(position, 2, 2))
				{
					var dust = Dust.NewDustPerfect(position, Main.rand.Next([DustID.Torch, DustID.FlameBurst]), Scale: Main.rand.NextFloat(0.5f, 2));
					dust.noGravity = true;
					dust.fadeIn = 1f;
					dust.velocity = dust.position.DirectionTo(Projectile.Center).RotatedBy(MathHelper.PiOver2);
				}
			}

			if (Main.rand.NextBool(3))
			{
				Vector2 position = Projectile.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(Projectile.width / 2);
				if (!Collision.SolidCollision(position, 2, 2))
				{
					var dust = Dust.NewDustPerfect(position, Main.rand.Next([DustID.Torch, DustID.FlameBurst]), Scale: Main.rand.NextFloat(0.5f, 2));
					dust.noGravity = true;
					dust.fadeIn = 1f;
					dust.velocity = dust.position.DirectionTo(Projectile.Center);
				}
			}

			if (++Projectile.frameCounter >= 4)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = ++Projectile.frame % Main.projFrames[Type];
			}

			GrantBuffs();
		}

		private void GrantBuffs()
		{
			const int time = 200;

			if (Projectile.owner == Main.myPlayer && ++Projectile.localAI[0] % 30 != 0)
			{
				Player owner = Main.player[Projectile.owner];
				HashSet<int> buffTypes = [];

				if (owner.GetPassiveStrength<FlameSageTree, FlameWard>() > 0)
				{
					buffTypes.Add(ModContent.BuffType<FlameWardBuff>());
				}

				if (owner.GetPassiveStrength<FlameSageTree, IntenseHeat>() > 0)
				{
					buffTypes.Add(ModContent.BuffType<IntenseHeatBuff>());
				}

				if (buffTypes.Count == 0)
				{
					return;
				}

				foreach (Player p in Main.ActivePlayers)
				{
					if (p.Hitbox.Intersects(Projectile.Hitbox))
					{
						foreach (int buffType in buffTypes)
						{
							p.AddBuff(buffType, time);
						}
					}
				}

				foreach (NPC n in Main.ActiveNPCs)
				{
					if (n.friendly && n.Hitbox.Intersects(Projectile.Hitbox))
					{
						foreach (int buffType in buffTypes)
						{
							n.AddBuff(buffType, time);
						}
					}
				}
			}
		}

		public override bool? CanCutTiles()
		{
			return false;
		}
	}
}