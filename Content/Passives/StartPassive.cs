using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class StartPassive : Passive
{
	public override bool CanDeallocate(Player player)
	{
		return false;
	}
}