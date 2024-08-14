using System.Collections.Generic;
using System.Drawing.Drawing2D;
using Terraria.ID;
using Terraria.Localization;

namespace PathOfTerraria.Common.Systems.Affixes;

public class AffixTooltipsHandler
{
	public readonly Dictionary<Type, AffixTooltip> Tooltips = [];

	public void Add(Type type, AffixTooltip.AffixSource source, float value, LocalizedText text, AffixTooltip.OverrideStringDelegate overrideString = null)
	{
		Tooltips.Add(type, new AffixTooltip()
		{
			Text = text,
			ValueBySource = new Dictionary<AffixTooltip.AffixSource, float>() { { source, value } },
			OriginalValueBySource = new Dictionary<AffixTooltip.AffixSource, float>() { { source, value } },
			OverrideString = overrideString,
			Color = Color.Green
		});
	}

	public void AddOrModify(Type type, Item source, float value, LocalizedText text, AffixTooltip.OverrideStringDelegate overrideString = null)
	{
		AddOrModify(type, DetermineItemSource(source), value, text, overrideString);
	}

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
			
			if (tooltip.OriginalValueBySource[source] > tooltip.ValueBySource[source])
			{
				tooltip.Color = Color.Red;
			}
		}
		else
		{
			Add(type, source, value, text, overrideString);
		}
	}

	internal void ModifyTooltips(List<TooltipLine> tooltips)
	{
		int tipNum = 0;

		foreach (KeyValuePair<Type, AffixTooltip> tip in Tooltips)
		{
			tooltips.Add(new TooltipLine(ModContent.GetInstance<PoTMod>(), "Tip" + tipNum++, $"[i:{ItemID.MusketBall}] " + tip.Value.Get())
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