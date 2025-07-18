using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Projectiles;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.Skills.Ranged;
using Terraria.ID;

namespace PathOfTerraria.Content.SkillPassives.RainOfArrowsTree;

internal class LingeringPoison(SkillTree tree) : SkillPassive(tree)
{
	internal class SporeCloud : SkillProjectile<RainOfArrows>
	{
		private ref float Timer => ref Projectile.ai[0];

		private float MaxTimeLeft => Skill.Tree.CountStrength<PowerfulSmog>() * 60 + 120;

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.Size = new Vector2(32, 28);
			Projectile.tileCollide = false;
			Projectile.timeLeft = 2;
		}

		public override bool? CanHitNPC(NPC target)
		{
			return false;
		}

		public override void AI()
		{
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.CanBeChasedBy() && npc.DistanceSQ(Projectile.Center) < 40 * 40)
				{
					// Add megatoxin buff directly to damage to simulate increase without having to pass values or check every frame
					SporeNPC.AddSporeDebuff(npc, Projectile.damage * Skill.Tree.CountStrength<Megatoxin>(), 4 * 60, true);
				}
			}

			Projectile.velocity *= 0.999f;
			Projectile.Opacity = 1 - Timer++ / MaxTimeLeft;
			Projectile.timeLeft = 2;

			if (Timer > MaxTimeLeft)
			{
				Projectile.Kill();
			}
		}
	}
}
