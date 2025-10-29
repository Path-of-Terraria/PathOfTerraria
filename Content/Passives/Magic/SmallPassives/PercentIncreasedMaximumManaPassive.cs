using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

// {0}% Increased Maximum Mana
internal class PercentIncreasedMaximumManaPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.statManaMax2 = (int)(player.statManaMax2 * (1.0f + (Value / 100.0f) * Level));
	}
}
