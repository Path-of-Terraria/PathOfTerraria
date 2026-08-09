using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class MaximumSentryPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.maxTurrets += (int)(Level * Value);
	}
}
