using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives.Melee.Masteries;

internal class ImmolationStrideMastery : Passive
{
	public override void BuffPlayer(Player player)
	{
		if (player.HasBuff<IgnitedDebuff>())
		{
			player.moveSpeed += 1;
			player.GetModPlayer<ElementalPlayer>().Container[ElementType.Fire].Multiplier *= 1.3f;
		}
	}
}
