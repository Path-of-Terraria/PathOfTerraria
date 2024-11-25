using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using PathOfTerraria.Common.Systems;

using TooltipUI = PathOfTerraria.Common.UI.Tooltip;

namespace PathOfTerraria.Content.Items.Gear;

public abstract class Gear : ModItem, GenerateAffixes.IItem, GenerateImplicits.IItem, PostRoll.IItem, GearLocalizationCategory.IItem, IPoTGlobalItem
{
	protected virtual string GearLocalizationCategory => GetType().Name;

	string GearLocalizationCategory.IItem.GetGearLocalizationCategory(string defaultCategory)
	{
		return GearLocalizationCategory;
	}

	public virtual List<ItemAffix> GenerateAffixes()
	{
		return [];
	}

	public virtual List<ItemAffix> GenerateImplicits()
	{
		return [];
	}

	public virtual void PostRoll() { }

	public override void HoldItem(Player player)
	{
		if (Item == Main.mouseItem || Item == player.inventory[58])
		{
			List<DrawableTooltipLine> tooltipLines = ItemTooltipBuilder.BuildTooltips(Item, player);
			TooltipUI.SetFancyTooltip(tooltipLines[1..]);
			TooltipUI.SetName($"[c/{tooltipLines[0].Color.Hex3()}:{tooltipLines[0].Text}]");
		}
	}
}