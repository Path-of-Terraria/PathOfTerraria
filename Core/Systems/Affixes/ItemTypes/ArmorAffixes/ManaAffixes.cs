namespace PathOfTerraria.Core.Systems.Affixes.ItemTypes.ArmorAffixes;

internal class ManaAffix : ItemAffix
{
	public override ItemType PossibleTypes => ItemType.Armor;
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.MaximumMana.Base += 5 + Value * 20 + item.ItemLevel / 20;
	}
}

internal class ManaRegenAffix : ItemAffix
{
	public override ItemType PossibleTypes => ItemType.Armor;
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.ManaRegen.Base += 1f + Value * 4f + item.ItemLevel / 40f;
	}
}

internal class ManaPotionPowerAffix : ItemAffix
{
	public override ItemType PossibleTypes => ItemType.Armor;
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.PotionManaPower.Base += 10f + Value * 10f + item.ItemLevel / 20f;
	}
}

internal class ManaPotionCapAffix : ItemAffix
{
	public override ItemType PossibleTypes => ItemType.Armor;
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.MaxManaPotions.Base += 1 + Value + item.ItemLevel / 100f;
	}
}

internal class ManaPotionCooldownAffix : ItemAffix
{
	public override ItemType PossibleTypes => ItemType.Armor;
	public override void ApplyAffix(EntityModifier modifier, PoTItem item)
	{
		modifier.PotionManaDelay.Base -= 60 * (0.5f + Value * 0.5f);
	}
}