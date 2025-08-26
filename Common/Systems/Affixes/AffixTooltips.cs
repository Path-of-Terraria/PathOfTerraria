using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PathOfTerraria.Common.AccessorySlots;
using PathOfTerraria.Content.Items.Gear.Rings;
using PathOfTerraria.Core.Items;
using Terraria.ID;
using Terraria.Localization;

#nullable enable

namespace PathOfTerraria.Common.Systems.Affixes;

/// <summary> Data related to construction of an individual affix's tooltip line.  </summary>
public struct AffixTooltipLine()
{
	public required LocalizedText Text;
	public LocalizedText? TextWhenRemoved;
	public float Value;
	public bool Corrupt;
	public Color? OverrideColor;
}

/// <summary>
/// Accumulates and stores affix tooltip information for specific items.
/// </summary>
public sealed class AffixTooltips
{
	private enum AffixSource : byte
	{
		MainItem,
		Helmet,
		Body,
		Legs,
		Wings,
		Necklace,
		RingAny, // There is two Ring slots, but comparisons can only be done with the first.
		Offhand,
		NonApplicable,
	}

	internal static Color DefaultColor = ItemTooltips.Colors.DefaultText;

	public readonly Dictionary<Type, AffixTooltipLine> Lines = [];

	/// <summary>
	/// Determines the <see cref="AffixSource"/> of the given item. <br/>
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	private static AffixSource DetermineItemSource(Item source)
	{
		// TODO: RingOn, RingOff, Necklace

		AffixSource result;

		if (source.headSlot > 0)
		{
			result = AffixSource.Helmet;
		}
		else if (source.bodySlot > 0)
		{
			result = AffixSource.Body;
		}
		else if (source.legSlot > 0)
		{
			result = AffixSource.Legs;
		}
		else if (source.wingSlot > 0)
		{
			result = AffixSource.Wings;
		}
		else if (source.accessory && source.neckSlot > 0)
		{
			result = AffixSource.Necklace;
		}
		else if (source.damage > 0 && source.ammo <= AmmoID.None)
		{
			result = AffixSource.MainItem;
		}
		else if (source.ModItem is Ring)
		{
			result = AffixSource.RingAny;
		}
		else
		{
			result = AffixSource.NonApplicable;
		}

		return result;
	}

	/// <summary> Adds a given <see cref="AffixTooltipLine"/> to this handler, stacking it with the . </summary>
	/// <exception cref="ArgumentException"/>
	public void AddOrModify(Type affixType, AffixTooltipLine tooltip)
	{
		if (!typeof(ItemAffix).IsAssignableFrom(affixType))
		{
			throw new ArgumentException("Type must be an ItemAffix child!", nameof(affixType));
		}

		if (Lines.TryGetValue(affixType, out AffixTooltipLine existing))
		{
			tooltip.Value += existing.Value;
		}

		Lines[affixType] = tooltip;
	}

	/// <summary>
	/// Adds all affix tooltips. Tooltips are named "AffixX".<br/>
	/// Additionally, adds in a (Hold shift to compare) tooltip &amp; allows for comparison &amp; non-comparison pages.
	/// </summary>
	/// <param name="tooltips">List to add to.</param>
	public void ModifyTooltips(List<TooltipLine> tooltips, Item item, Player player)
	{
		AffixSource source = DetermineItemSource(item);
		_ = player;

		Item? comparisonItem = null;
		bool canCompareEver = source is not AffixSource.NonApplicable;
		bool canCompareNow = canCompareEver && TryFindComparisonItem(source, player, out comparisonItem);
		bool shouldCompare = canCompareNow && Main.keyState.PressingShift();

		int tipNum = 0;
		AddTooltipLines(tooltips, ref tipNum);

		// Show a notice notifying the user that they can hold shift to compare with equipped item, or that they are, or that they cannot.
		if (canCompareEver)
		{
			if (!canCompareNow)
			{
				tooltips.Add(new TooltipLine(PoTMod.Instance, "ShiftNotice", Language.GetTextValue($"Mods.{PoTMod.ModName}.TooltipNotices.NoSwap")) { OverrideColor = Color.DarkGray });
			}
			else if (shouldCompare)
			{
				tooltips.Add(new TooltipLine(PoTMod.Instance, "ShiftNotice", Language.GetTextValue($"Mods.{PoTMod.ModName}.TooltipNotices.Swap", item.Name)) { OverrideColor = Color.Gray });
			}
			else
			{
				tooltips.Add(new TooltipLine(PoTMod.Instance, "ShiftNotice", Language.GetTextValue($"Mods.{PoTMod.ModName}.TooltipNotices.Shift")) { OverrideColor = Color.Gray });
			}
		}

		if (shouldCompare)
		{
			AffixTooltips otherTooltips = CollectAffixTooltips(comparisonItem!, player);
			AffixTooltips comparison = CreateComparison(this, otherTooltips);

			int oldTipNum = tipNum;
			comparison.AddTooltipLines(tooltips, ref tipNum);

			if (tipNum == oldTipNum)
			{
				tooltips.Add(new TooltipLine(PoTMod.Instance, "ComparisonNoDifferences", "No differences") { OverrideColor = ItemTooltips.Colors.DefaultText });
			}
		}
	}

	private void AddTooltipLines(List<TooltipLine> tooltips, ref int tipNum)
	{
		foreach (AffixTooltipLine tip in Lines.Values)
		{
			string text = $"{ItemTooltips.ColoredDot(ItemTooltips.Colors.AffixAccent)} {tip.Text.WithFormatArgs(Math.Abs(tip.Value).ToString("#0.##"), tip.Value >= 0 ? "+" : "-").Value}";
			Color color = tip.Value switch
			{
				_ when tip.OverrideColor.HasValue => tip.OverrideColor.Value,
				_ when tip.Corrupt => ItemTooltips.Colors.Corrupt,
				> 0f => ItemTooltips.Colors.Positive,
				< 0f => ItemTooltips.Colors.Negative,
				_ => ItemTooltips.Colors.DefaultNumber,
			};

			tooltips.Add(new TooltipLine(PoTMod.Instance, "Affix" + tipNum, text) { OverrideColor = color });
			tipNum++;
		}
	}

	public static AffixTooltips CollectAffixTooltips(Item item, Player player)
	{
		var handler = new AffixTooltips();

		foreach (ItemAffix affix in item.GetInstanceData().Affixes)
		{
			affix.ApplyTooltip(player, item, handler);
		}

		return handler;
	}

	public static AffixTooltips CreateComparison(AffixTooltips a, AffixTooltips b)
	{
		var comparison = new AffixTooltips();

		foreach ((Type affixType, AffixTooltipLine tooltipA) in a.Lines)
		{
			if (b.Lines.TryGetValue(affixType, out AffixTooltipLine tooltipB))
			{
				// Both items have this affix, denote the value difference.
				float difference = tooltipA.Value - tooltipB.Value;

				comparison.AddOrModify(affixType, tooltipA with
				{
					Value = difference,
				});
			}
			else
			{
				// This affix will be new if equipped, denote it as is.
				comparison.AddOrModify(affixType, tooltipA);
			}
		}

		foreach ((Type affixType, AffixTooltipLine tooltipB) in b.Lines)
		{
			if (!a.Lines.ContainsKey(affixType))
			{
				// This affix will be missing if equipped, denote it as gone or negative.
				comparison.AddOrModify(affixType, tooltipB with
				{
					Text = tooltipB.TextWhenRemoved ?? tooltipB.Text,
					Value = -tooltipB.Value,
				});
			}
		}

		return comparison;
	}

	private static bool TryFindComparisonItem(AffixSource source, Player player, [NotNullWhen(true)] out Item? result)
	{
		result = source switch
		{
			AffixSource.MainItem => player.inventory[0],
			AffixSource.Helmet => player.armor[(int)RemappedEquipSlots.Head],
			AffixSource.Body => player.armor[(int)RemappedEquipSlots.Body],
			AffixSource.Legs => player.armor[(int)RemappedEquipSlots.Legs],
			AffixSource.Wings => player.armor[(int)RemappedEquipSlots.Wings],
			AffixSource.Necklace => player.armor[(int)RemappedEquipSlots.Necklace],
			AffixSource.Offhand => player.armor[(int)RemappedEquipSlots.Offhand],
			AffixSource.RingAny => player.armor[(int)RemappedEquipSlots.RingOn],
			_ => null,
		};

		return result?.IsAir == false;
	}
}