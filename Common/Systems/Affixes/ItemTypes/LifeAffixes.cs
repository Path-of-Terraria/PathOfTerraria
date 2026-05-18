namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal abstract class LifeAffix : ItemAffix
{
	protected LifeAffix()
	{
		Round = true;
	}
}

internal class BaseLifeAffix : LifeAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.MaximumLife.Base += Value;
	}
}

internal class AddedLifeAffix : LifeAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.MaximumLife += Value / 100;
	}
}

internal class LifeRegenAffix : LifeAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.LifeRegen.Base += Value;
	}
}

internal class LifeRegenMultiplierAffix : LifeAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.LifeRegen += Value / 100;
	}
}

internal class LifePotionPowerAffix : LifeAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.PotionHealPower.Base += Value;
	}
}

internal class LifePotionCapAffix : LifeAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.MaxHealthPotions += Value;
	}
}

internal class LifePotionCooldownAffix : LifeAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.PotionHealDelay.Base -= Value;
	}
}
