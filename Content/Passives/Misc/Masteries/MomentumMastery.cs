using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Misc.Masteries;

internal class MomentumMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<MomentumPlayer>().hasMomentum = true;
	}

	internal class MomentumPlayer : ModPlayer
	{
		public bool hasMomentum = false;

		public override void ResetEffects()
		{
			hasMomentum = false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			// If target didnt die, return
			if (target.life > 0)
			{
				return;
			}
			
			// Ignore critters
			if (!hasMomentum || target.lifeMax <= 5) 
			{
				return;
			}
	
			// Add 0.2 seconds to all active buffs
			const int bonusTime = 12;

			for (int i = 0; i < Player.MaxBuffs; i++)
			{
				if (Player.buffTime[i] > 0 && !Main.debuff[Player.buffType[i]])
				{
					Player.buffTime[i] += bonusTime;
				}
			}
		}
	}
}
