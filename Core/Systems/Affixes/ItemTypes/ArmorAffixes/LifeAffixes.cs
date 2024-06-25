namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes.ArmorAffixes;

internal class BaseLifeAffix : ItemAffix
{
	public override ItemType PossibleTypes => ItemType.Armor;

	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.MaximumLife.Base += 10f + Value * 30f + item.ItemLevel / 100f;
	}
}
internal class AddedLifeAffix : ItemAffix
{
	public override ItemType PossibleTypes => ItemType.Armor;

	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.MaximumLife += (1f + Value) * item.ItemLevel / 10f / 100f;
	}
}
internal class MultipliedLifeAffix : ItemAffix
{
	public override ItemType PossibleTypes => ItemType.Armor;

	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.MaximumLife *= 1f + (1f + Value) * item.ItemLevel / 20f / 100f;
	}
}
internal class FlatLifeAffix : ItemAffix
{
	public override ItemType PossibleTypes => ItemType.Armor;

	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.MaximumLife.Flat += 4f + (Value + 0.5f) * item.ItemLevel / 10;
	}
}

internal class LifeRegenAffix : ItemAffix
{
	public override ItemType PossibleTypes => ItemType.Chestplate;

	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.LifeRegen.Base += 1f + Value * 4f + item.ItemLevel / 40f;
	}
}

internal class LifeRegenMultiplierAffix : ItemAffix
{
	public override ItemType PossibleTypes => ItemType.Armor;

	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.LifeRegen += (1f + Value + item.ItemLevel) / 170f;
	}
}

internal class LifePotionPowerAffix : ItemAffix
{
	public override ItemType PossibleTypes => ItemType.Armor;

	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.PotionHealPower.Base += 10f + Value * 10f + item.ItemLevel / 20f;
	}
}

internal class LifePotionCapAffix : ItemAffix
{
	public override ItemType PossibleTypes => ItemType.Armor;

	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.MaxHealthPotions.Base += 1 + Value + item.ItemLevel / 100f;
	}
}

internal class LifePotionCooldownAffix : ItemAffix
{
	public override ItemType PossibleTypes => ItemType.Armor;

	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.PotionHealDelay.Base -= 60 * (0.5f + Value * 0.5f);
	}
}