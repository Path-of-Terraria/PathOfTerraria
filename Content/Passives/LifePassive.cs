using PathOfTerraria.Core.Systems.TreeSystem;
using System.Collections.Generic;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems.ModPlayers;

namespace PathOfTerraria.Content.Passives;

internal class LifePassive : Passive
{
	public LifePassive()
	{
		Name = "Empowered Flesh";
		Tooltip = "Increases your maximum life by 20 per level";
		MaxLevel = 5;
		TreePos = new Vector2(400, 400);
		Classes = [PlayerClass.Melee, PlayerClass.Magic, PlayerClass.Ranged, PlayerClass.Summoner];
	}

	public override void BuffPlayer(Player player)
	{
		player.statLifeMax2 += 20 * Level;
	}

	public override void Connect(List<Passive> all, Player player)
	{
		ClassModPlayer mp = player.GetModPlayer<ClassModPlayer>();
		switch (mp.SelectedClass)
		{
			case PlayerClass.Melee:
				Connect<MartialMasteryPassive>(all, player);
				break;
			case PlayerClass.Ranged:
				Connect<MarksmanshipMasteryPassive>(all, player);
				Connect<CloseRangePassive>(all, player);
				break;
			case PlayerClass.Magic:
				Connect<ArcaneMasteryPassive>(all, player);
				break;
			case PlayerClass.Summoner:
				Connect<SummoningMasteryPassive>(all, player);
				break;
		}
	}
}