using PathOfTerraria.Core.Systems.TreeSystem;
using System.Collections.Generic;
using PathOfTerraria.Content.Items.Gear;

namespace PathOfTerraria.Content.Passives;

internal class StartPassive : Passive
{
	public StartPassive()
	{
		Name = "Anchor";
		Tooltip = "Your journey starts here";
		Level = 1;
		MaxLevel = 1;

		Width = 58;
		Height = 58;
		TreePos = new Vector2(400, 500);
		Classes = [PlayerClass.Melee, PlayerClass.Magic, PlayerClass.Ranged, PlayerClass.Summoner];
	}

	public override void Connect(List<Passive> all, Player player)
	{
		Connect<LifePassive>(all, player);
	}

	public override bool CanDeallocate(Player player)
	{
		return false;
	}
}