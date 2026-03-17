using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddMaxRagePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<RagePlayer>().MaxRage += Value;
	}
}

