using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class FasterMiningPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.pickSpeed *= 1 + Value / 100f;
	}
}
