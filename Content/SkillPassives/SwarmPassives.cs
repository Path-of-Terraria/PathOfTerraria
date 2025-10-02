using PathOfTerraria.Common;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.Skills.Summon;
using PathOfTerraria.Content.SkillTrees;
using Terraria.ID;

namespace PathOfTerraria.Content.SkillPassives.SwarmPassives;

internal class AggressiveChill(SkillTree tree) : SkillPassive(tree);

internal class BiggerBrood(SkillTree tree) : SkillPassive(tree);

internal class CarapaceCracker(SkillTree tree) : SkillPassive(tree)
{
	internal class CrackedCarapaceDebuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.debuff[Type] = true;
		}
	}

	internal class CrackedCarapaceNPC : GlobalNPC
	{
		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
		{
			if (npc.HasBuff<CrackedCarapaceDebuff>())
			{
				modifiers.DefenseEffectiveness *= 0.5f;
			}
		}
	}
}

internal class CarnivorousLarvae(SkillTree tree) : SkillPassive(tree);

internal class ColdBlooded(SkillTree tree) : SkillPassive(tree);

internal class CombustableGuts(SkillTree tree) : SkillPassive(tree);

internal class Eggsplosion(SkillTree tree) : SkillPassive(tree);

internal class FrostbiteMandibles(SkillTree tree) : SkillPassive(tree);

internal class Gestation(SkillTree tree) : SkillPassive(tree);

internal class HeartierExplosions(SkillTree tree) : SkillPassive(tree);

internal class IceVenom(SkillTree tree) : SkillPassive(tree);

internal class InfectedDetonation(SkillTree tree) : SkillPassive(tree);

internal class OverheatingBugs(SkillTree tree) : SkillPassive(tree);

internal class QuickerHatching(SkillTree tree) : SkillPassive(tree);

internal class ShatteringCarapace(SkillTree tree) : SkillPassive(tree)
{
	internal class IceShards : SkillProjectile<PestSwarm>
	{
		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 3;
		}

		public override void SetDefaults()
		{
			Projectile.Size = new Vector2(20);
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 600;
			Projectile.penetrate = 5;
			Projectile.frame = Main.rand.Next(3);
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.2f;
			Projectile.rotation += Projectile.velocity.X * 0.07f;

			if (Projectile.timeLeft < 60)
			{
				Projectile.Opacity = Projectile.timeLeft / 60f;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
			{
				Projectile.velocity.X = -oldVelocity.X;
			}

			if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
			{
				Projectile.velocity.X *= 0.99f;
			}

			return false;
		}
	}
}

internal class ShockingEmergence(SkillTree tree) : SkillPassive(tree);

internal class SuperheatedBugs(SkillTree tree) : SkillPassive(tree);

internal class ThermalConversion(SkillTree tree) : SkillPassive(tree);

internal class ViciousBites(SkillTree tree) : SkillPassive(tree);

internal class VolatileInsects(SkillTree tree) : SkillPassive(tree)
{
	internal class AntlionExplosion : ExplosionHitboxFriendly
	{
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Player owner = Main.player[Projectile.owner];
			float addedChance = owner.GetPassiveStrength<PestSwarmTree, CombustableGuts>() * 0.02f;

			if (owner.HasTreePassive<PestSwarmTree, OverheatingBugs>())
			{
				IgnitedDebuff.ApplyTo(target, damageDone);
			}

			if (target.life <= 0 && owner.HasTreePassive<PestSwarmTree, InfectedDetonation>() && Main.rand.NextFloat() < 0.1f + addedChance)
			{
				int exp = ModContent.ProjectileType<AntlionExplosion>();
				int damage = (int)(target.life * 0.1f + owner.GetPassiveStrength<PestSwarmTree, HeartierExplosions>() * 0.02f);
				int proj = Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, exp, damage, 8, Projectile.owner, 60, 60);
				Main.projectile[proj].GetGlobalProjectile<ElementalProjectile>().Container[ElementType.Fire].DamageModifier.AddModifiers(0, 1f);

				VFX(Projectile, new VFXPackage(4, 12, 6, true, 0.8f, null));

				for (int i = 0; i < 12; ++i)
				{
					Vector2 vel = Main.rand.NextVector2Circular(6, 6) + target.velocity;
					Dust.NewDust(target.position, target.width, target.height, DustID.Lava, vel.X, vel.Y);
				}
			}
		}
	}
}
