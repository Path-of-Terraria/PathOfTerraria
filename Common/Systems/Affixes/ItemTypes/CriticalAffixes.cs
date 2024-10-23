namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class PercentageIncreasedCriticalStrikeChanceAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.CriticalChance.Base += modifier.CriticalChance.Base * (Value / 100);
	}
}

internal class PercentageCriticalStrikeMultiplierAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.CriticalMultiplier *= Value / 100;
	}
}

internal class FlatCriticalStrikeChanceAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.CriticalChance.Flat += Value;
	}
}

internal class PercentageIncreasedCriticalStrikeDamageAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.CriticalDamage.Base += modifier.CriticalChance.Base * (Value / 100);
	}
}
