using System.Collections.Generic;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Affixes;

public class AffixTooltipsHandler
{
	public readonly Dictionary<Type, AffixTooltip> Tooltips = [];

	public void Add(Type type, float value, LocalizedText text, AffixTooltip.OverrideStringDelegate overrideString = null)
	{
		Tooltips.Add(type, new AffixTooltip()
		{
			Text = text,
			Value = value,
			OverrideString = overrideString,
			Color = Color.Green
		});
	}

	public void AddOrModify(Type type, float value, LocalizedText text, AffixTooltip.OverrideStringDelegate overrideString = null)
	{
		if (Tooltips.TryGetValue(type, out AffixTooltip tooltip))
		{
			tooltip.Value += value;
		}
		else
		{
			Add(type, value, text, overrideString);
		}
	}

	internal void ModifyTooltips(Item item, List<TooltipLine> tooltips)
	{
		int tipNum = 0;

		foreach (KeyValuePair<Type, AffixTooltip> tip in Tooltips)
		{
			tooltips.Add(new TooltipLine(ModContent.GetInstance<PoTMod>(), "Tip" + tipNum++, tip.Value.Get())
			{
				OverrideColor = tip.Value.Color
			});
		}
	}

	internal void Reset()
	{
		Tooltips.Clear();
	}
}