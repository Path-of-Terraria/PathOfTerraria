using PathOfTerraria.Content.Items.Gear;

namespace PathOfTerraria.Core.Systems.Affixes.Affixes.GearTypes.ArmorAffixes;

internal class BaseLifeAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override void ApplyAffix(EntityModifier modifier, Gear gear)
	{
		modifier.MaximumLife.Base += 10f + Value * 30f + gear.ItemLevel / 100f;
	}
}
internal class AddedLifeAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override void ApplyAffix(EntityModifier modifier, Gear gear)
	{
		modifier.MaximumLife += (1f + Value) * gear.ItemLevel / 10f / 100f;
	}
}
internal class MultipliedLifeAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override void ApplyAffix(EntityModifier modifier, Gear gear)
	{
		modifier.MaximumLife *= 1f + (1f + Value) * gear.ItemLevel / 20f / 100f;
	}
}
internal class FlatLifeAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override void ApplyAffix(EntityModifier modifier, Gear gear)
	{
		modifier.MaximumLife.Flat += 4f + (Value + 0.5f) * gear.ItemLevel / 10;
	}
}

internal class LifeRegenAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override void ApplyAffix(EntityModifier modifier, Gear gear)
	{
		modifier.LifeRegen.Base += 1f + Value * 4f + gear.ItemLevel / 40f;
	}
}

internal class LifePotionPowerAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override void ApplyAffix(EntityModifier modifier, Gear gear)
	{
		modifier.PotionHealPower.Base += 10f + Value * 10f + gear.ItemLevel / 20f;
	}
}

internal class LifePotionCapAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override void ApplyAffix(EntityModifier modifier, Gear gear)
	{
		modifier.MaxHealthPotions.Base += 1 + Value + gear.ItemLevel / 100f;
	}
}

internal class LifePotionCooldownAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override void ApplyAffix(EntityModifier modifier, Gear gear)
	{
		modifier.PotionHealDelay.Base -= 60 * (0.5f + Value * 0.5f);
	}
}