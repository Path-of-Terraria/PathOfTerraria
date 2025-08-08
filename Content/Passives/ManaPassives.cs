using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

// -5% Mana Costs
internal class DecreasedManaCostPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.manaCost *= 1 - 0.05f * Level;
	}
}

// +20 Increased Maximum Mana
internal class AddedManaPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.statManaMax2 += 20 * Level;
	}
} 

// 5% Increased Maximum Mana
internal class PercentIncreasedMaximumManaPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.statManaMax2 = (int)(player.statManaMax2 * (1.0f + 0.05f * Level));
	}
}

