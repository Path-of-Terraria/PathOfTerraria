using PathOfTerraria.Common.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class LifePassive : Passive
{
	public override string InternalIdentifier => "AddedLife";
	
	public override void BuffPlayer(Player player)
	{
		player.statLifeMax2 += 20 * Level;
	}
}

internal class LifeRegenPassive : Passive
{
	public override string InternalIdentifier => "AddedLifeRegen";

	public override void BuffPlayer(Player player)
	{
		player.lifeRegen += 2 * Level;
	}
}