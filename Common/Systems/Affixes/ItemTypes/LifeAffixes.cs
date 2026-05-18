namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal abstract class LifeAffix : ItemAffix
{
	protected LifeAffix()
	{
		Round = true;
	}

	protected float RoundedValue => (float)Math.Round(Value);

	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return base.CreateDefaultTooltip(player, item) with { Value = (int)Math.Round(Value) };
	}
}

internal class BaseLifeAffix : LifeAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.MaximumLife.Base += RoundedValue;
	}
}

internal class AddedLifeAffix : LifeAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.MaximumLife += RoundedValue / 100;
	}
}

internal class LifeRegenAffix : LifeAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.LifeRegen.Base += RoundedValue;
	}
}

internal class LifeRegenMultiplierAffix : LifeAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.LifeRegen += RoundedValue / 100;
	}
}

internal class LifePotionPowerAffix : LifeAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.PotionHealPower.Base += RoundedValue;
	}
}

internal class LifePotionCapAffix : LifeAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.MaxHealthPotions += RoundedValue;
	}
}

internal class LifePotionCooldownAffix : LifeAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.PotionHealDelay.Base -= RoundedValue;
	}
}
