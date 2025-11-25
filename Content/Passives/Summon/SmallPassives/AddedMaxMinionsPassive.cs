using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddedMaxMinionsPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.maxMinions += Value;
	}
}