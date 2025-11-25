using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives;

internal class FasterGrimoirePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<GrimoirePlayer>().Stats.SpeedModifier += Value / 100f;
	}
}