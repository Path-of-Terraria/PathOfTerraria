using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.DataStructures;

namespace PathOfTerraria.Content.Passives.Summon.Masteries;

internal class SacrificialSummonsMastery : Passive
{
	internal class SacrificialSummonsPlayer : ModPlayer
	{
		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genDust, ref PlayerDeathReason damageSource)
		{
			if (Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<SacrificialSummonsMastery>(out float value) && Player.slotsMinions >= value)
			{
				foreach (Projectile proj in Main.ActiveProjectiles)
				{
					if (proj.owner == Player.whoAmI && proj.minion && proj.minionSlots > 0)
					{
						value -= proj.minionSlots;
					}

					if (value <= 0)
					{
						break;
					}
				}

				Player.statLife = 50;
				Player.SetImmuneTimeForAllTypes(120);
				return false;
			}

			return true;
		}
	}
}