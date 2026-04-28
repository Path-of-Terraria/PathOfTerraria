using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives;

internal class StrongerPoisonPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<PoisonedDebuff.PoisonPlayer>().PoisonDamage += Value / 100f;
	}
}