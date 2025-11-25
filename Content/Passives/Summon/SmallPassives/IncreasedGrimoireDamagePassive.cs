using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.ID;

namespace PathOfTerraria.Content.Passives;

internal class IncreasedGrimoireDamagePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<GrimoirePlayer>().Stats.DamageModifier += Value / 100f;
	}
}