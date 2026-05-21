using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Misc;

internal class DebuffsExpireFasterPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		float modifier = Math.Max(0f, 1f - (Value / 100f));
		player.GetModPlayer<BuffModifierPlayer>().ResistanceStrength *= modifier;
	}
}