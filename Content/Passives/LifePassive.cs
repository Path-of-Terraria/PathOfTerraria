using PathOfTerraria.Core.Systems.TreeSystem;
using System.Collections.Generic;

namespace PathOfTerraria.Content.Passives;

internal class LifePassive : Passive
{
	public LifePassive()
	{
		Name = "Empowered Flesh";
		Tooltip = "Increases your maximum life by 20 per level";
		MaxLevel = 5;
		TreePositions = [
			new Vector2(400, 400),
			new Vector2(200, 600)
		];
	}

	public override void BuffPlayer(Player player)
	{
		player.statLifeMax2 += 20 * Level;
	}

	public override void Connect(List<Passive> all, Player player)
	{
		Connect<MeleePassive>(all, player);
		Connect<RangedPassive>(all, player);
		Connect<MagicPassive>(all, player);
		Connect<SummonPassive>(all, player);
	}
}