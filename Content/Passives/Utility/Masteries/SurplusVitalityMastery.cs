using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class SurplusVitalityMastery : Passive
{
	internal class SurplusVitalityPlayer : ModPlayer, SkillHooks.IOnUseSkillPlayer
	{
		public void OnUseSkill(Skill skill)
		{
			if (!Player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<SurplusVitalityMastery>(out float value))
			{
				return;
			}

			foreach (Player player in Main.ActivePlayers)
			{
				if (player.DistanceSQ(Player.Center) < PoTMod.NearbyDistanceSq)
				{
					player.GetModPlayer<OverhealthPlayer>().SetOverhealth((int)(Player.statLifeMax2 * value / 100f));
				}
			}
		}
	}
}
