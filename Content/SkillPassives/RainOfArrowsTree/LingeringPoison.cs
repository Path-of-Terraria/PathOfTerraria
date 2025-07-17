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
		public int MaxTimeLeft = 2 * 60;

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.Size = new Vector2(32, 28);
			Projectile.tileCollide = false;
			Projectile.timeLeft = MaxTimeLeft;
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
					npc.AddBuff(BuffID.Poisoned, 4 * 60);
				}
			}

			Projectile.velocity *= 0.999f;
			Projectile.Opacity = Projectile.timeLeft / (float)MaxTimeLeft;
		}
	}
}
