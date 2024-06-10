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

	public override void ConnectMelee(List<Passive> all, Player player)
	{
		Connect<MartialMasteryPassive>(all, player);
	}
	
	public override void ConnectRanged(List<Passive> all, Player player)
	{
		Connect<MarksmanshipMasteryPassive>(all, player);
		Connect<CloseRangePassive>(all, player);
	}
	
	public override void ConnectMagic(List<Passive> all, Player player)
	{
		Connect<ArcaneMasteryPassive>(all, player);
	}
	
	public override void ConnectSummoner(List<Passive> all, Player player)
	{
		Connect<SummoningMasteryPassive>(all, player);
	}
}