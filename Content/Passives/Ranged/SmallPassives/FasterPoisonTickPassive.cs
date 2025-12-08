using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives;

internal class FasterPoisonTickPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<PoisonedDebuff.PoisonPlayer>().PoisonTickRate *= 1 - Value / 100f;
	}
}