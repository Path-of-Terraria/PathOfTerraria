using PathOfTerraria.Common.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class AddedLifePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.statLifeMax2 += 20 * Level;
	}
}

internal class LifeRegenPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.lifeRegen += 2 * Level;
	}
}