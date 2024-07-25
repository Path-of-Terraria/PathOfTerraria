namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class ManaAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.MaximumMana.Base += Value;
	}
}

internal class ManaRegenAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.ManaRegen.Base += Value;
	}
}

internal class ManaPotionPowerAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.PotionManaPower.Base += Value;
	}
}

internal class ManaPotionCapAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.MaxManaPotions.Base += Value;
	}
}

internal class ManaPotionCooldownAffix : ItemAffix
{
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.PotionManaDelay.Base -= Value;
	}
}