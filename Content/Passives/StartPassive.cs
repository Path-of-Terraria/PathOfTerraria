using PathOfTerraria.Core.Systems.TreeSystem;
using System.Collections.Generic;
using PathOfTerraria.Content.Items.Gear;

namespace PathOfTerraria.Content.Passives;

internal class StartPassive : Passive
{
	public override string Name => "Anchor";
	public override string Tooltip => "Your journey starts here";
	public override bool CanDeallocate(Player player)
	{
		return false;
	}
}