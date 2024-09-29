namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class PercentageIncreasedCriticalStrikeChance : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.CriticalChance.Base += modifier.CriticalChance.Base * (Value / 100);
	}
}

internal class PercentageCriticalStrikeMultiplier : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.CriticalMultiplier *= Value / 100;
	}
}

internal class FlatCriticalStrikeChance : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.CriticalChance.Flat += Value;
	}
}

internal class PercentageIncreasedCriticalStrikeDamage : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.CriticalDamage.Base += modifier.CriticalChance.Base * (Value / 100);
	}
}
