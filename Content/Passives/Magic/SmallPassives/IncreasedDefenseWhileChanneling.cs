using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

public class IncreasedDefenseWhileChanneling : Passive
{
	public override void BuffPlayer(Player player)
	{
		if (player.channel)
		{
			player.statDefense += (int)(Value * Level);
		}
	}
}
