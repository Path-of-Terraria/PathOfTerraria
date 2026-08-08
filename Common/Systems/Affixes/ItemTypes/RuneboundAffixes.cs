using PathOfTerraria.Common.Systems.ElementalDamage;
using PathOfTerraria.Common.Systems.ModPlayers;

namespace PathOfTerraria.Common.Systems.Affixes.ItemTypes;

/// <summary>Craft-only affixes supplied by the rarest Runebound encounters.</summary>
internal sealed class RuneboundPrismaticAffix : ItemAffix
{
	public RuneboundPrismaticAffix()
	{
		Round = true;
	}

	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		foreach (ElementInstance element in player.GetModPlayer<ElementalPlayer>().Container)
		{
			element.Resistance += Value / 100f;
		}
	}
}

internal sealed class RuneboundTitanAffix : ItemAffix
{
	public RuneboundTitanAffix()
	{
		Round = true;
	}

	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.MaximumLife.Base += Value;
	}
}

internal sealed class RuneboundAnnihilationAffix : ItemAffix
{
	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		modifier.Damage += Value / 100f;
	}
}

internal sealed class RuneboundTrinityAffix : ItemAffix
{
	public RuneboundTrinityAffix()
	{
		Round = true;
	}

	public override void ApplyAffix(Player player, EntityModifier modifier, Item item)
	{
		AttributesPlayer attributes = player.GetModPlayer<AttributesPlayer>();
		attributes.Strength += (int)Value;
		attributes.Dexterity += (int)Value;
		attributes.Intelligence += (int)Value;
	}
}
