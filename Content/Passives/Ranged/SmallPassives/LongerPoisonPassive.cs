using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives;

internal class LongerPoisonPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<PoisonedDebuff.PoisonPlayer>().PoisonDuration += Value / 100f;
	}
}