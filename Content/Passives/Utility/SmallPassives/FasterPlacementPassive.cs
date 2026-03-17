using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class FasterPlacementPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.tileSpeed *= 1 + Value / 100f;
	}
}
