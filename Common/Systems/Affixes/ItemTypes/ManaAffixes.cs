namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal abstract class ManaItemAffix : ItemAffix
{
	protected ManaItemAffix()
	{
		Round = true;
	}

	protected float RoundedValue => (float)Math.Round(Value);

	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return base.CreateDefaultTooltip(player, item) with { Value = (int)Math.Round(Value) };
	}
}

internal class ManaAffix : ManaItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.MaximumMana.Base += RoundedValue;
	}
}

internal class ManaRegenAffix : ManaItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.ManaRegen.Base += RoundedValue;
	}
}

internal class ManaPotionPowerAffix : ManaItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.PotionManaPower.Base += RoundedValue;
	}
}

internal class ManaPotionCapAffix : ManaItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.MaxManaPotions += RoundedValue;
	}
}

internal class ManaPotionCooldownAffix : ManaItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.PotionManaDelay.Base -= RoundedValue;
	}
}
