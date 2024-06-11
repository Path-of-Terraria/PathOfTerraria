using PathOfTerraria.Core.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class StartPassive : Passive
{
	public override int Id => 0;
	public override string Name => "Anchor";
	public override string Tooltip => "Your journey starts here";
	
	public override bool CanDeallocate(Player player)
	{
		return false;
	}
}