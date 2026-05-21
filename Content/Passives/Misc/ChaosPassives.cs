using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;

namespace PathOfTerraria.Content.Passives.Misc;

internal class ReducedMaxLifeIncreasedChaosDamagePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		float lifeModifier = Math.Max(0f, 1f - (Value / 100f));
		player.statLifeMax2 = (int)(player.statLifeMax2 * lifeModifier);

		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.Container[ElementType.Chaos].Multiplier *= 1f + (Value / 100f);
	}
}