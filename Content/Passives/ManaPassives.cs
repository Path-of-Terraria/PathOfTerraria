using PathOfTerraria.Common.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class DecreasedManaCostPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.manaCost *= 1 - 0.03f * Level; //3% per level
	}
}

internal class AddedManaPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.statManaMax2 += 20 * Level;
	}
}