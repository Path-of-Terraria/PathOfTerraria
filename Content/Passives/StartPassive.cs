using PathOfTerraria.Common.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class StartPassive : Passive
{
	public override string InternalIdentifier => "Anchor";
	
	public override bool CanDeallocate(Player player)
	{
		return false;
	}
}