using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class StrongerDebuffsPassive : Passive
{
	internal class StrongDebuffPlayer : ModPlayer
	{
		public override void Load()
		{
			On_NPC.AddBuff += ModifyDebuffDuration;
		}

		private void ModifyDebuffDuration(On_NPC.orig_AddBuff orig, NPC self, int type, int time, bool quiet)
		{
			if (Main.debuff[type])
			{
				float debuffStrength = 0;

				foreach (Player player in Main.ActivePlayers)
				{
					if (player.GetModPlayer<PassiveTreePlayer>().TryGetCumulativeValue<StrongerDebuffsPassive>(out float value))
					{
						debuffStrength += value / 100f;
					}
				}

				time = (int)(time * (1 + debuffStrength));
			}

			orig(self, type, time, quiet);
		}
	}
}