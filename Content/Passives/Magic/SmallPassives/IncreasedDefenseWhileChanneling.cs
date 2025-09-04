using PathOfTerraria.Common.Events;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

public class IncreasedDefenseWhileChanneling : Passive
{
	public override void BuffPlayer(Player player)
	{
		if (player.channel)
		{
			player.statDefense += Value * Level;
		}
	}
}