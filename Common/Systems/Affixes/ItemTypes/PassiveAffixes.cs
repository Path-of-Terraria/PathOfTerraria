using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

/// <summary>
/// Increases the player's attack speed by a percentage (Value).
/// </summary>
internal class IncreasedAttackSpeedAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.AttackSpeed += Value / 100f;
	}
}

/// <summary>
/// Adds a flat percentage (Value) to the player's base attack speed.
/// </summary>
internal class AddedAttackSpeedAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.AttackSpeed.Base += Value / 100f;
	}
}

/// <summary>
/// Adds a flat amount (Value) to the player's base damage.
/// </summary>
internal class AddedDamageAffix : ItemAffix
{
	public AddedDamageAffix()
	{
		Round = true;
	}
	
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Damage.Base += Value;
	}
}

/// <summary>
/// Increases the player's damage by a percentage (Value / 500).
/// </summary>
internal class IncreasedDamageAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Damage += Value / 500;
	}

	protected override AffixTooltipLine CreateDefaultTooltip(Player player, Item item)
	{
		return base.CreateDefaultTooltip(player, item) with { Value = Value / 5f };
	}
}

internal class AmmoReservationAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetModPlayer<AmmoConsumptionPlayer>().AmmoSaveChance += Value / 100f;
	}
}

internal class IncreasedSummonDamageAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetDamage(DamageClass.Summon) +=  Value / 100f;
	}
}

internal class IncreasedMeleeDamageAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetDamage(DamageClass.Melee) += Value / 100f;
	}
}

internal class IncreasedRangedDamageAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetDamage(DamageClass.Ranged) += Value / 100f;
	}
}

internal class IncreasedMagicDamageAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		player.GetDamage(DamageClass.Magic) += Value / 100f;
	}
}

internal class AddedSummonCritAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.SummonCritChance += Value / 100f;
	}
}