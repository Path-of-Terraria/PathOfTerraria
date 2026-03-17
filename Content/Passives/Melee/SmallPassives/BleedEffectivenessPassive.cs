using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives;

internal class BleedEffectivenessPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<BleedPlayer>().BleedEffectiveness += Value / 100f;
	}
}
