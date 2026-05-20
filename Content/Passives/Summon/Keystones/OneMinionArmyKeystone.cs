using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Summon.Keystones;

public class OneMinionArmyKeystone : Passive
{
	internal class OneMinionArmyPlayer : ModPlayer
	{
		private int storedMaxMinions = 0;

		public override void PostUpdateEquips()
		{
			if (Player.GetModPlayer<PassiveTreePlayer>().HasNode<OneMinionArmyKeystone>())
			{
				storedMaxMinions = Player.maxMinions;
                
				Player.maxMinions = 1;
			}
		}

		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			if (proj.minion && Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<OneMinionArmyKeystone>(out float value))
			{
				float damageMultiplier = storedMaxMinions * value / 100f;
				modifiers.FinalDamage += damageMultiplier;
			}
		}
	}
}