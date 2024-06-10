using PathOfTerraria.Core.Systems.TreeSystem;
using System.Collections.Generic;
using PathOfTerraria.Content.Items.Gear;
using PathOfTerraria.Core.Systems.ModPlayers;

namespace PathOfTerraria.Content.Passives;

internal class LifePassive : Passive
{
	public override string Name => "Empowered Flesh";
	public override string Tooltip => "Increases your maximum life by 20 per level";
	public override void BuffPlayer(Player player)
	{
		player.statLifeMax2 += 20 * Level;
	}
}