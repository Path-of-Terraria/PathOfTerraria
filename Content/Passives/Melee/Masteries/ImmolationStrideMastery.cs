using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives.Melee.Masteries;

internal class ImmolationStrideMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		if (player.HasBuff<IgnitedDebuff>() || player.HasBuff<ScorchingFlamesDebuff>())
		{
			float value = Value / 100f;
			player.moveSpeed += value;
			player.GetModPlayer<ElementalPlayer>().Container[ElementType.Fire].Multiplier *= (1 + value);
		}
	}
}
