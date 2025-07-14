﻿namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

internal class BaseLifeAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.MaximumLife.Base += Value;
	}
}

internal class AddedLifeAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.MaximumLife += Value / 100;
	}
}

internal class MultipliedLifeAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.MaximumLife *= 1 + Value / 100;
	}
}

internal class FlatLifeAffix : ItemAffix
{
	public FlatLifeAffix()
	{
		Round = true;
	}
	
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.MaximumLife.Flat += Value;
	}

	public override void ApplyTooltip(Player player, Item item, AffixTooltipsHandler handler)
	{
		handler.AddOrModify(GetType(), item, (int)Math.Round(Value), this.GetLocalization("Description"), IsCorruptedAffix);
	}
}

internal class LifeRegenAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.LifeRegen.Base += Value;
	}
}

internal class LifeRegenMultiplierAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.LifeRegen += Value / 100;
	}
}

internal class LifePotionPowerAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.PotionHealPower.Base += Value;
	}
}

internal class LifePotionCapAffix : ItemAffix
{
	public LifePotionCapAffix()
	{
		Round = true;
	}
	
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.MaxHealthPotions.Base += Value;
	}
}

internal class LifePotionCooldownAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.PotionHealDelay.Base -= Value;
	}
}