namespace PathOfTerraria.Common.Systems.ModPlayers;

public class BuffModifierPlayer : ModPlayer
{
	public StatModifier ResistanceStrength = StatModifier.Default;
	public StatModifier BuffBonus = StatModifier.Default;

	public override void Load()
	{
		On_Player.AddBuff += BoostBuffsAndReduceDebuffs;
	}

	private void BoostBuffsAndReduceDebuffs(On_Player.orig_AddBuff orig, Player self, int type, int timeToAdd, bool quiet, bool foodHack)
	{
		if (timeToAdd > 10)
		{
			if (Main.debuff[type])
			{
				timeToAdd = (int)self.GetModPlayer<BuffModifierPlayer>().ResistanceStrength.ApplyTo(timeToAdd);
			}
			else
			{
				timeToAdd = (int)self.GetModPlayer<BuffModifierPlayer>().BuffBonus.ApplyTo(timeToAdd);
			}
		}

		orig(self, type, timeToAdd, quiet, foodHack);
	}

	public override void ResetEffects()
	{
		ResistanceStrength = BuffBonus = StatModifier.Default;
	}
}
