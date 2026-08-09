using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class FasterPlacementPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.tileSpeed += Value / 100f;
	}
}
