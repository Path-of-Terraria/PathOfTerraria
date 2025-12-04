using PathOfTerraria.Core.Items;

namespace PathOfTerraria.Common.Systems.ElementalDamage;

public enum ElementType : byte
{
	None,
	Fire,
	Cold,
	Lightning,
	Chaos,
}

public static class ElementExtensions
{
	public static Color ElementColor(this ElementType type)
	{
		return type switch
		{
			ElementType.Fire => ItemTooltips.Colors.FireDamage,
			ElementType.Cold => ItemTooltips.Colors.ColdDamage,
			ElementType.Lightning => ItemTooltips.Colors.LightningDamage,
			ElementType.Chaos => ItemTooltips.Colors.ChaosDamage,
			_ => ItemTooltips.Colors.DefaultNumber
		};
	}
}