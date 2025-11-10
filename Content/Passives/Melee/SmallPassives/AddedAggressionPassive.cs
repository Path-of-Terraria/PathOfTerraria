using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddedAggressionPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.aggro += 1 * Level;
	}
}
