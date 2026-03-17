using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class HeartAndMindMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.manaRegen += player.lifeRegen / 2;
	}
}
