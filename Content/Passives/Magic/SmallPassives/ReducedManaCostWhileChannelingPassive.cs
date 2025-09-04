using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class ReducedManaCostWhileChannelingPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		if (player.channel)
		{
			player.manaCost *= 1 - (Value/100.0f) * Level;
		}
	}
}