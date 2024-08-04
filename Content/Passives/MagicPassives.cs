﻿using PathOfTerraria.Common.Systems.TreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class ManaPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.statManaMax2 += 20 * Level;
	}
}