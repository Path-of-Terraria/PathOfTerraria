using PathOfTerraria.Common;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.SkillPassives.FlameSage;
using PathOfTerraria.Content.Skills.Summon;
using PathOfTerraria.Content.SkillTrees;
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
			NPC.Opacity = Math.Min(NPC.Opacity + 0.08f, 1);

			int slowBurn = Owner.GetPassiveStrength<FlameSageTree, SlowBurn>();
			if (slowBurn > 0)
			{
				Rectangle hitbox = NPC.Hitbox;
				hitbox.Inflate(100, 100);

				int accelerant = Owner.GetPassiveStrength<FlameSageTree, Accelerant>();
				int dotDamage = (int)((NPC.damage * 0.2f * slowBurn) * (1 + (accelerant * Accelerant.DamageIncrease)));

				FlamethrowerNPC.DealDoT(hitbox, dotDamage, static (npc) =>
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
		}

		public override void OnKill()
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				Projectile.NewProjectile(NPC.GetSource_Death(), NPC.Center, Vector2.Zero, ProjectileID.DaybreakExplosion, NPC.damage, 3, Owner.whoAmI);
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
}