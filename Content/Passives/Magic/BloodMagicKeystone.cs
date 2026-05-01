using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Magic;

public class BloodMagicKeystone : Passive
{
	internal class BloodMagicPlayer : ModPlayer
	{
		private int lifeStealCooldown = 0;
		
		public override void PostUpdate()
		{
			if (lifeStealCooldown > 0)
			{
				lifeStealCooldown--;
			}
		}

		public override void PostUpdateEquips()
		{
			if (Player.GetModPlayer<PassiveTreePlayer>().HasNode<BloodMagicKeystone>())
			{
				Player.manaCost = 0f;
			}
		}

		public override bool CanUseItem(Item item)
		{
			if (Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<BloodMagicKeystone>(out float value) && item.mana > 0)
			{
				int lifeCost = item.mana;
				if (Player.statLife <= lifeCost)
				{
					return false;
				}
				
				Player.statLife -= lifeCost;
			}
			return base.CanUseItem(item);
		}

		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (proj.DamageType == DamageClass.Magic && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<BloodMagicKeystone>(out float value))
			{
				modifiers.ModifyHitInfo += (ref NPC.HitInfo hitInfo) =>
				{
					if (lifeStealCooldown > 0)
						return;
					
					float hpScaling = Math.Min(1.0f, 600f / Player.statLifeMax2);
					float effectiveHealRate = (value / 100f) * hpScaling;
					
					int lifeSteal = Math.Max(0, (int)Math.Ceiling(hitInfo.Damage * effectiveHealRate));
					
					int maxHealPerHit = Math.Max(5, Player.statLifeMax2 / 20);
					lifeSteal = Math.Min(lifeSteal, maxHealPerHit);
					
					if (lifeSteal > 0)
					{
						Player.statLife = Math.Min(Player.statLife + lifeSteal, Player.statLifeMax2);
						Player.HealEffect(lifeSteal);
						lifeStealCooldown = 3;
					}
				};
			}
		}
	}
}