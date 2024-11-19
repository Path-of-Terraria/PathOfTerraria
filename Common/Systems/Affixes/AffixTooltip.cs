using PathOfTerraria.Common.Systems.Affixes.ItemTypes;
using System.Collections.Generic;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Affixes;

/// <summary>
/// Controls an individual affix's tooltip. Includes tooling for comparison, overriding display text, and tracking sources.
/// </summary>
public class AffixTooltip
{
	public enum AffixSource : byte 
	{
		MainItem,
		Helmet,
		Body,
		Legs,
		Wings,
		Necklace,
		RingOn,
		RingOff
	}

	public delegate string OverrideStringDelegate(AffixTooltip self, float value, float difference, float originalValue, LocalizedText text);

	public string DisplayValue => AggregateValue().ToString("#0.##");
	public string DisplayDifference => AggregateDifference().ToString("+#0.##;-#0.##;0");
	public string DisplaySign => AggregateValue() >= 0 ? "+" : "-";
	public bool HasDifference => SourceItems.Count > 1 && AnyDifferenceInValues();

	public readonly HashSet<Item> SourceItems = [];

	public LocalizedText Text;
	public Dictionary<AffixSource, float> ValueBySource = [];
	public Dictionary<AffixSource, float> OriginalValueBySource = [];
	public OverrideStringDelegate OverrideString = null;
	public Color Color = Color.WhiteSmoke;
	public bool Corrupt = false;

	/// <summary>
	/// Total value of this <see cref="AffixTooltip"/>.
	/// </summary>
	/// <returns>Total value from all sources.</returns>
	public float AggregateValue()
	{
		float value = 0;

		foreach (float item in ValueBySource.Values)
		{
			value += item;
		}

		return value;
	}

	/// <summary>
	/// Total original value of this <see cref="AffixTooltip"/>. Used for comparisons.
	/// </summary>
	/// <returns>Total original value from all sources.</returns>
	public float AggregateOriginalValue()
	{
		float value = 0;

		foreach (float item in OriginalValueBySource.Values)
		{
			value += item;
		}

		return value;
	}

	/// <summary>
	/// Total difference between original and current value on this <see cref="AffixTooltip"/>. Used for comparisons.
	/// </summary>
	/// <returns>Total difference from all sources.</returns>
	public float AggregateDifference()
	{
		float value = 0;

		foreach ((AffixSource key, float item) in ValueBySource)
		{
			value += item;

			if (OriginalValueBySource[key] == ValueBySource[key])
			{
				continue;
			}

			value -= OriginalValueBySource[key];
		}

		return value;
	}

	/// <summary>
	/// Returns the final display string for this given <see cref="AffixTooltip"/>. <br/>
	/// The return of this may be overriden with <see cref="OverrideString"/>. See <see cref="NoFallDamageAffix"/> for implementation.
	/// </summary>
	/// <returns>Final display string.</returns>
	public string Get()
	{
		float realValue = AggregateValue();
		float differenceValue = AggregateDifference();
		bool hasOverride = OverrideString is not null;
		
		if (hasOverride)
		{
			return OverrideString(this, realValue, differenceValue, AggregateOriginalValue(), Text);
		}

		string baseText = Text.WithFormatArgs(DisplayValue, DisplaySign).Value;

		if (differenceValue != realValue)
		{
			baseText = Text.WithFormatArgs(DisplayDifference, "").Value;
		}

		return baseText;
	}

	/// <summary>
	/// Resets all values on this <see cref="AffixTooltip"/>.
	/// </summary>
	internal void ClearValues(AffixSource source)
	{
		if (!ValueBySource.ContainsKey(source))
		{
			return;
		}

		float originalValue = AggregateValue();
		ValueBySource[source] = 0;
		Color = originalValue < AggregateValue() ? Color.Green : Color.Red;
	}

	/// <summary>
	/// Compares all values with their original values to see if there is an actual difference.
	/// </summary>
	/// <returns></returns>
	private bool AnyDifferenceInValues()
	{
		foreach (AffixSource key in ValueBySource.Keys)
		{
			if (ValueBySource[key] != OriginalValueBySource[key])
			{
				return true;
			}
		}

		return false;
	}
}