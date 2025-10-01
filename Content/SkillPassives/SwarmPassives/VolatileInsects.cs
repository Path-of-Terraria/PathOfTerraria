using PathOfTerraria.Common;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.Buffs.ElementalBuffs;
using PathOfTerraria.Content.Projectiles.Utility;
using PathOfTerraria.Content.SkillTrees;
using Terraria.ID;

namespace PathOfTerraria.Content.SkillPassives.SwarmPassives;

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
				int damage = (int)(target.life * 0.1f + (owner.GetPassiveStrength<PestSwarmTree, HeartierExplosions>() * 0.02f));
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
