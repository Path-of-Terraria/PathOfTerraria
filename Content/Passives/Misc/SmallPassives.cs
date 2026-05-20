using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Content.Buffs.ElementalBuffs;

namespace PathOfTerraria.Content.Passives;

internal class DebuffsExpireFasterPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		float modifier = Math.Max(0f, 1f - (Value / 100f) * Level);
		player.GetModPlayer<BuffModifierPlayer>().ResistanceStrength *= modifier;
	}
}

internal class AddedChaosDamageMultiplierPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.Container[ElementType.Chaos].Multiplier *= 1f + (Value / 100f) * Level;
	}
}

internal class AddedChaosDamageConversionPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		ref ElementalDamage self = ref elemental.Container[ElementType.Chaos].DamageModifier;
		self = self.AddModifiers(null, (Value / 100f) * Level);
	}
}

internal class ReducedMaxLifeIncreasedChaosDamagePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		float lifeModifier = Math.Max(0f, 1f - (Value / 100f) * Level);
		player.statLifeMax2 = (int)(player.statLifeMax2 * lifeModifier);

		ElementalPlayer elemental = player.GetModPlayer<ElementalPlayer>();
		elemental.Container[ElementType.Chaos].Multiplier *= 1f + (Value / 100f) * Level;
	}
}

internal class IncreasedIgniteDamagePassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<IgnitedPlayer>().IgniteDamage += (Value / 100f) * Level;
	}
}

internal class IgnitesDealDamageFasterPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		float modifier = Math.Max(0f, 1f - (Value / 100f) * Level);
		player.GetModPlayer<IgnitedPlayer>().IgniteDuration *= modifier;
	}
}

internal class AddedCriticalStrikeMultiplierPassive : Passive
{
	public override void BuffPlayer(Player player)
	{
		player.GetModPlayer<UniversalBuffingPlayer>().UniversalModifier.CriticalMultiplier *= 1f + (Value / 100f) * Level;
	}
}
