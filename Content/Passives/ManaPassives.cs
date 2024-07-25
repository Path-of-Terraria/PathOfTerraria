using PathOfTerraria.Common.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class DecreasedManaCostPassive : Passive
{
	public override string InternalIdentifier => "DecreasedManaCost";
	
	public override void BuffPlayer(Player player)
	{
		player.manaCost *= 1 - 0.03f * Level; //3% per level
	}
}