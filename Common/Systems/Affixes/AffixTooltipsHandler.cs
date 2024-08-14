using System.Collections.Generic;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Affixes;

/// <summary>
/// Handles an individual player's <see cref="AffixTooltip"/>s.
/// </summary>
public class AffixTooltipsHandler
{
	internal static Color DefaultColor = Color.WhiteSmoke;

	public readonly Dictionary<Type, AffixTooltip> Tooltips = [];

	/// <summary>
	/// Adds a <see cref="AffixTooltip"/> with the given parameters.
	/// </summary>
	/// <param name="type">The type to use as a lookup. Should be an ItemAffix.</param>
	/// <param name="source">Source of the tooltip. Use <see cref="DetermineItemSource(Item)"/> if you only have an item.</param>
	/// <param name="value">Initial value of the new tooltip.</param>
	/// <param name="text">Localized text of the new tooltip.</param>
	/// <param name="overrideString">Overriden functionality of the tooltip's result. Defaults to null, which uses default functionality.</param>
	/// <exception cref="ArgumentException"></exception>
	public void Add(Type type, AffixTooltip.AffixSource source, float value, LocalizedText text, AffixTooltip.OverrideStringDelegate overrideString = null)
	{
		if (!typeof(ItemAffix).IsAssignableFrom(type))
		{
			throw new ArgumentException("Type must be an ItemAffix child!", nameof(type));
		}

		Tooltips.Add(type, new AffixTooltip()
		{
			Text = text,
			ValueBySource = new Dictionary<AffixTooltip.AffixSource, float>() { { source, value } },
			OriginalValueBySource = new Dictionary<AffixTooltip.AffixSource, float>() { { source, value } },
			OverrideString = overrideString,
			Color = DefaultColor
		});
	}

	/// <summary>
	/// Adds or modifies a <see cref="AffixTooltip"/>. This uses <paramref name="source"/> for <see cref="DetermineItemSource(Item)"/>.
	/// </summary>
	/// <param name="type">The type to use as a lookup. Should be an ItemAffix.</param>
	/// <param name="source">Item who called this. Will run <see cref="DetermineItemSource(Item)"/> for the actual source.</param>
	/// <param name="value">Initial value of the new tooltip.</param>
	/// <param name="text">Localized text of the new tooltip.</param>
	/// <param name="overrideString">Overriden functionality of the tooltip's result. Defaults to null, which uses default functionality.</param>
	public void AddOrModify(Type type, Item source, float value, LocalizedText text, AffixTooltip.OverrideStringDelegate overrideString = null)
	{
		AddOrModify(type, DetermineItemSource(source), value, text, overrideString);
	}

	/// <summary>
	/// Determines the <see cref="AffixTooltip.AffixSource"/> of the given item. <br/>
	/// Currently doesn't support <see cref="AffixTooltip.AffixSource.RingOff"/>, <see cref="AffixTooltip.AffixSource.RingOn"/> or <see cref="AffixTooltip.AffixSource.Necklace"/>.
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	public static AffixTooltip.AffixSource DetermineItemSource(Item source)
	{
		// TODO: RingOn, RingOff, Necklace

		if (source.headSlot > 0)
		{
			return AffixTooltip.AffixSource.Helmet;
		}
		else if (source.bodySlot > 0)
		{
			return AffixTooltip.AffixSource.Body;
		}
		else if (source.legSlot > 0)
		{
			return AffixTooltip.AffixSource.Legs;
		}
		else if (source.wingSlot > 0)
		{
			return AffixTooltip.AffixSource.Wings;
		}

		return AffixTooltip.AffixSource.MainItem;
	}

	public void AddOrModify(Type type, AffixTooltip.AffixSource source, float value, LocalizedText text, AffixTooltip.OverrideStringDelegate overrideString = null)
	{
		if (Tooltips.TryGetValue(type, out AffixTooltip tooltip))
		{
			tooltip.ValueBySource[source] = value;

			if (tooltip.ValueBySource[source] == tooltip.OriginalValueBySource[source])
			{
				tooltip.Color = DefaultColor;
			}
			else
			{
				tooltip.Color = tooltip.OriginalValueBySource[source] > tooltip.ValueBySource[source] ? Color.Red : Color.Green;
			}
		}
		else
		{
			Add(type, source, value, text, overrideString);
		}
	}

	/// <summary>
	/// Adds all tooltips. Tooltips are named "Affix[x]" and have a Musket Ball sprite prepended.
	/// </summary>
	/// <param name="tooltips">List to add to.</param>
	internal void ModifyTooltips(List<TooltipLine> tooltips)
	{
		int tipNum = 0;

		foreach (KeyValuePair<Type, AffixTooltip> tip in Tooltips)
		{
			tooltips.Add(new TooltipLine(ModContent.GetInstance<PoTMod>(), "Affix" + tipNum++, $"[i:{ItemID.MusketBall}] " + tip.Value.Get())
			{
				OverrideColor = tip.Value.Color,
			});
		}
	}

	/// <summary>
	/// Clears the tooltips.
	/// </summary>
	internal void Reset()
	{
		Tooltips.Clear();
	}
}