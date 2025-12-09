using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives;

internal class BleedTickPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<BleedPlayer>().TickCountModifier *= 1 - Value / 100f;
	}
}
