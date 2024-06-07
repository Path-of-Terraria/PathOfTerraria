using PathOfTerraria.Content.Items.Gear;

namespace PathOfTerraria.Core.Systems.Affixes.Affixes.GearTypes.ArmorAffixes;

internal class ManaAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override void ApplyAffix(EntityModifier modifier, Gear gear)
	{
		modifier.MaximumMana.Base += 5 + Value * 20 + gear.ItemLevel / 20;
	}
}

internal class ManaRegenAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override void ApplyAffix(EntityModifier modifier, Gear gear)
	{
		modifier.ManaRegen.Base += 1f + Value * 4f + gear.ItemLevel / 40f;
	}
}

internal class ManaPotionPowerAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override void ApplyAffix(EntityModifier modifier, Gear gear)
	{
		modifier.PotionManaPower.Base += 10f + Value * 10f + gear.ItemLevel / 20f;
	}
}

internal class ManaPotionCapAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override void ApplyAffix(EntityModifier modifier, Gear gear)
	{
		modifier.MaxManaPotions.Base += 1 + Value + gear.ItemLevel / 100f;
	}
}

internal class ManaPotionCooldownAffix : GearAffix
{
	public override GearType PossibleTypes => GearType.Armor;
	public override void ApplyAffix(EntityModifier modifier, Gear gear)
	{
		modifier.PotionManaDelay.Base -= 60 * (0.5f + Value * 0.5f);
	}
}