using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.Skills.Summon;
using Terraria.Audio;
using Terraria.ID;

namespace PathOfTerraria.Content.SkillSpecials.FlameSageSpecials;

public class Flamethrower(SkillTree tree) : SkillSpecial(tree)
{
	public class FlamethrowerSentry : FlameSage.FlameSentry
	{
		public override void SetDefaults()
		{
			base.SetDefaults();
			NPC.damage = 4;
		}

		public override void AI()
		{
			const int cooldownMax = 10;
			const int range = 100;

			if ((Cooldown = Math.Max(Cooldown - 1, 0)) == 0)
			{
				Terraria.Utilities.NPCUtils.TargetSearchResults results = FindTarget();
				NPC target = results.NearestNPC;

				if (results.FoundNPC && Collision.CanHit(NPC, target) && target.DistanceSQ(NPC.Center) < range * range)
				{
					int damage = (int)Owner.GetDamage(DamageClass.Summon).ApplyTo(NPC.damage);
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, NPC.DirectionTo(target.Center) * 2, ProjectileID.Flames, damage, 1, Owner.whoAmI);

					Cooldown = cooldownMax;
				}
			}

			if (NPC.Opacity == 0) //Just spawned in
			{
				SoundEngine.PlaySound(SoundID.DD2_WyvernDiveDown, NPC.Center);
			}

			NPC.Opacity = Math.Min(NPC.Opacity + 0.08f, 1);
		}
	}
}