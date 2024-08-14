using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Affixes;

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
	public string DisplayDifference => AggregateDifference().ToString("+#0.##;-#0.##");
	public string DisplaySign => AggregateValue() >= 0 ? "+" : "-";

	public LocalizedText Text;
	public Dictionary<AffixSource, float> ValueBySource = [];
	public Dictionary<AffixSource, float> OriginalValueBySource = [];
	public OverrideStringDelegate OverrideString = null;
	public Color Color = Color.Green;

	public float AggregateValue()
	{
		float value = 0;

		foreach (float item in ValueBySource.Values)
		{
			value += item;
		}

		return value;
	}

	public float AggregateOriginalValue()
	{
		float value = 0;

		foreach (float item in OriginalValueBySource.Values)
		{
			value += item;
		}

		return value;
	}

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
			baseText += $" ({DisplayDifference})";
		}

		return baseText;
	}

	internal void ClearValues()
	{
		foreach (AffixSource key in ValueBySource.Keys)
		{
			ValueBySource[key] = 0;
		}
	}
}