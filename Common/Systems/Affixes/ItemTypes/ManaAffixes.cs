namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal abstract class ManaItemAffix : ItemAffix
{
	protected ManaItemAffix()
	{
		Round = true;
	}
}

internal class ManaAffix : ManaItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.MaximumMana.Base += Value;
	}
}

internal class ManaRegenAffix : ManaItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.ManaRegen.Base += Value;
	}
}

internal class ManaPotionPowerAffix : ManaItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.PotionManaPower.Base += Value;
	}
}

internal class ManaPotionCapAffix : ManaItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.MaxManaPotions += Value;
	}
}

internal class ManaPotionCooldownAffix : ManaItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.PotionManaDelay.Base -= Value;
	}
}
