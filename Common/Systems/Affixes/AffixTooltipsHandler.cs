using Microsoft.Xna.Framework.Input;
using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using System.Linq;
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
	public void Add(Type type, AffixTooltip.AffixSource source, float value, LocalizedText text, bool corrupt, Item item, AffixTooltip.OverrideStringDelegate overrideString = null)
	{
		if (!typeof(ItemAffix).IsAssignableFrom(type))
		{
			throw new ArgumentException("Type must be an ItemAffix child!", nameof(type));
		}

		var tooltip = new AffixTooltip()
		{
			Text = text,
			ValueBySource = new Dictionary<AffixTooltip.AffixSource, float>() { { source, value } },
			OriginalValueBySource = new Dictionary<AffixTooltip.AffixSource, float>() { { source, value } },
			OverrideString = overrideString,
			Color = DefaultColor,
			Corrupt = corrupt,
		};

		if (item is not null)
		{
			tooltip.SourceItems.Add(item);
		}

		Tooltips.Add(type, tooltip);
	}

	/// <summary>
	/// Adds or modifies a <see cref="AffixTooltip"/>. This uses <paramref name="source"/> for <see cref="DetermineItemSource(Item)"/>.
	/// </summary>
	/// <param name="type">The type to use as a lookup. Should be an ItemAffix.</param>
	/// <param name="source">Item who called this. Will run <see cref="DetermineItemSource(Item)"/> for the actual source.</param>
	/// <param name="value">Initial value of the new tooltip.</param>
	/// <param name="text">Localized text of the new tooltip.</param>
	/// <param name="overrideString">Overriden functionality of the tooltip's result. Defaults to null, which uses default functionality.</param>
	public void AddOrModify(Type type, Item source, float value, LocalizedText text, bool corrupt, AffixTooltip.OverrideStringDelegate overrideString = null)
	{
		AddOrModify(type, DetermineItemSource(source), value, text, corrupt, source, overrideString);
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
		else if (source.accessory)
		{
			return AffixTooltip.AffixSource.Necklace;
		}

		return AffixTooltip.AffixSource.MainItem;
	}

	public void AddOrModify(Type type, AffixTooltip.AffixSource source, float value, LocalizedText text, bool corrupt, Item item, AffixTooltip.OverrideStringDelegate overrideString = null)
	{
		if (Tooltips.TryGetValue(type, out AffixTooltip tooltip))
		{
			if (!tooltip.ValueBySource.ContainsKey(source))
			{
				tooltip.OriginalValueBySource.Add(source, value);
				tooltip.ValueBySource.Add(source, 0);
			}

			tooltip.ValueBySource[source] = value;

			if (!tooltip.SourceItems.Any(HasSource(item)))
			{
				tooltip.SourceItems.Add(item);
			}

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
			Add(type, source, value, text, corrupt, item, overrideString);
		}
	}

	/// <summary>
	/// Quick check for if an affix tooltip's sources already has a given item,
	/// so they aren't listed as being from 1 item twice.
	/// </summary>
	/// <param name="item"></param>
	/// <returns></returns>
	private static Func<Item, bool> HasSource(Item item)
	{
		return x =>
		{
			bool value = !x.IsNotSameTypePrefixAndStack(item);
			PoTInstanceItemData data = x.GetInstanceData();
			PoTInstanceItemData itemData = item.GetInstanceData();

			return value && data.Rarity == itemData.Rarity && data.Affixes == itemData.Affixes;
		};
	}

	/// <summary>
	/// Adds all affix tooltips. Tooltips are named "AffixX" and have a Musket Ball sprite prepended.<br/>
	/// Additionally, adds in a (Hold shift to compare) tooltip &amp; allows for comparison &amp; non-comparison pages.
	/// </summary>
	/// <param name="tooltips">List to add to.</param>
	internal void ModifyTooltips(List<TooltipLine> tooltips, Item item)
	{
		int tipNum = 0;
		bool hasShift = Keyboard.GetState().PressingShift();
		IEnumerable<KeyValuePair<Type, AffixTooltip>> differenceTips = null;
		IEnumerable<KeyValuePair<Type, AffixTooltip>> firstTips = null;

		if (!hasShift) // If we're not holding shift, remove all tooltips & add "Shift to compare" line
		{
			firstTips = CreateStandaloneTooltips(item);
		}
		else // Otherwise, put the tooltips modified by the current item at the top of the list & re-generate the standalone lines
		{
			AffixTooltip.AffixSource source = DetermineItemSource(item);
			differenceTips = Tooltips.OrderByDescending(x => x.Value.SourceItems.Any(v => item.type == v.type) ? 1 : 0).Where(x => x.Value.ValueBySource.ContainsKey(source));
			differenceTips = new Dictionary<Type, AffixTooltip>(differenceTips); // Create a shallow clone of itself in order to de-reference from Tooltips

			for (int i = 0; i < differenceTips.Count(); ++i)
			{
				KeyValuePair<Type, AffixTooltip> pair = differenceTips.ElementAt(i);

				foreach (object val in Enum.GetValues(typeof(AffixTooltip.AffixSource)))
				{
					var enumVal = (AffixTooltip.AffixSource)val;

					if (enumVal != source)
					{
						pair.Value.OriginalValueBySource.Remove(enumVal);
						pair.Value.ValueBySource.Remove(enumVal);
					}
				}

				List<Item> removals = [];

				foreach (Item obj in pair.Value.SourceItems)
				{
					if (DetermineItemSource(obj) != source)
					{
						removals.Add(obj);
					}
				}

				foreach (Item obj in removals)
				{
					pair.Value.SourceItems.Remove(obj);
				}
			}

			Tooltips.Clear();
			firstTips = CreateStandaloneTooltips(item);
		}

		if (tooltips.Count == 0 && (differenceTips is null || !differenceTips.Any()) && !firstTips.Any())
		{
			return;
		}

		if (firstTips is not null)
		{
			foreach (KeyValuePair<Type, AffixTooltip> tip in firstTips)
			{
				AddSingleTooltipLine(tooltips, ref tipNum, tip);
			}
		}

		if (differenceTips is not null)
		{
			bool anyDif = false;

			foreach (KeyValuePair<Type, AffixTooltip> tip in differenceTips) // Determine if there is any difference
			{
				if (tip.Value.HasDifference)
				{
					anyDif = true;
					break;
				}
			}

			if (hasShift) // Display "If X is equipped:" or "(No item to swap)" in comparison page
			{
				string swapText = !anyDif ? Language.GetTextValue($"Mods.{PoTMod.ModName}.TooltipNotices.NoSwap")
					: Language.GetText($"Mods.{PoTMod.ModName}.TooltipNotices.Swap").Format(item.Name);

				tooltips.Add(new TooltipLine(PoTMod.Instance, "SwapNotice", swapText)
				{
					OverrideColor = Color.Gray,
				});
			}

			if (anyDif) // If there are differences, show comparison
			{
				foreach (KeyValuePair<Type, AffixTooltip> tip in differenceTips)
				{
					AddSingleTooltipLine(tooltips, ref tipNum, tip);
				}
			}
		}

		if (!hasShift) // Show "Shift to compare" if they're not doing so
		{
			tooltips.Add(new TooltipLine(PoTMod.Instance, "ShiftNotice", Language.GetTextValue($"Mods.{PoTMod.ModName}.TooltipNotices.Shift"))
			{
				OverrideColor = Color.Gray,
			});
		}
	}

	private static int AddSingleTooltipLine(List<TooltipLine> tooltips, ref int tipNum, KeyValuePair<Type, AffixTooltip> tip)
	{
		string text = $"[i:{ItemID.MusketBall}] " + tip.Value.Get();

		tooltips.Add(new TooltipLine(PoTMod.Instance, "Affix" + tipNum++, text)
		{
			OverrideColor = tip.Value.Corrupt ? Color.Lerp(Color.Purple, Color.White, 0.4f) : tip.Value.Color,
		});
		return tipNum;
	}

	private IEnumerable<KeyValuePair<Type, AffixTooltip>> CreateStandaloneTooltips(Item item)
	{
		Tooltips.Clear();
		PoTItemHelper.ApplyAffixTooltips(item, Main.LocalPlayer);
		return Tooltips;
	}

	/// <summary>
	/// Clears the tooltips.
	/// </summary>
	internal void Reset()
	{
		Tooltips.Clear();
	}
}