﻿using PathOfTerraria.Core.Systems.TreeSystem;
using System.Collections.Generic;

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
		TreePositions = [new Vector2(400, 500)];
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