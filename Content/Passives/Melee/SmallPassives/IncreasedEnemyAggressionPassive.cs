using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedEnemyAggressionPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.aggro += Value;
	}
}

