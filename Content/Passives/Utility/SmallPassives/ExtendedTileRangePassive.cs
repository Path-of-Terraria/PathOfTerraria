using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class ExtendedTileRangePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.blockRange += Value;
	}
}
