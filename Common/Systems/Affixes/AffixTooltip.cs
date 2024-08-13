using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Affixes;

public class AffixTooltip
{
	public delegate string OverrideStringDelegate(float value, LocalizedText text);

	public LocalizedText Text;
	public float Value = 0;
	public OverrideStringDelegate OverrideString = null;
	public Color Color = Color.Green;

	public string Get()
	{
		return OverrideString is not null ? OverrideString(Value, Text) : Text.WithFormatArgs(Value, Value >= 0 ? "+" : "-").Value;
	}
}