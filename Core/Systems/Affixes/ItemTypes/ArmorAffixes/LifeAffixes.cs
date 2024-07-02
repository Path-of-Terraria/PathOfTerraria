namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes.ArmorAffixes;

internal class BaseLifeAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.MaximumLife.Base += Value;
	}
}
internal class AddedLifeAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.MaximumLife += Value;
	}
}
internal class MultipliedLifeAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.MaximumLife *= Value;
	}
}
internal class FlatLifeAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.MaximumLife.Flat += Value;
	}
}

internal class LifeRegenAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.LifeRegen.Base += Value;
	}
}

internal class LifeRegenMultiplierAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.LifeRegen += Value;
	}
}

internal class LifePotionPowerAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.PotionHealPower.Base += 10f + Value * 10f + item.ItemLevel / 20f;
	}
}

internal class LifePotionCapAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.MaxHealthPotions.Base += 1 + Value + item.ItemLevel / 100f;
	}
}

internal class LifePotionCooldownAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.PotionHealDelay.Base -= 60 * (0.5f + Value * 0.5f);
	}
}