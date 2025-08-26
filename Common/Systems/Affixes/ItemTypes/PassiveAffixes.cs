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

	public override void ApplyTooltip(Player player, Item item, AffixTooltips handler)
	{
		handler.AddOrModify(GetType(), new AffixTooltipLine
		{
			Text = this.GetLocalization("Description"),
			Value = Value / 5f,
			Corrupt = IsCorruptedAffix,
		});
	}
}