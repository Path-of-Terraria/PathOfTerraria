using PathOfTerraria.Core.Items;
using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Affixes;
using Terraria.UI.Chat;
using PathOfTerraria.Common.Systems;

using TooltipUI = PathOfTerraria.Common.UI.Tooltip;

namespace PathOfTerraria.Content.Items.Gear;

public abstract class Gear : ModItem, GenerateAffixes.IItem, GenerateImplicits.IItem, PostRoll.IItem, GearLocalizationCategory.IItem
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
			string tooltip = string.Empty;
			List<DrawableTooltipLine> tooltipLines = ItemTooltipBuilder.BuildTooltips(Item, player);
			float width = 0;
			
			// Skip the first line as that's the name.
			// Add the name in later.

			for (int i = 1; i < tooltipLines.Count; ++i)
			{
				DrawableTooltipLine line = tooltipLines[i];
				tooltip += line.Text + (i != tooltipLines.Count - 1 ? "\n" : "");
				Vector2 size = ChatManager.GetStringSize(line.Font, line.Text, Vector2.One);

				if (size.X > width)
				{
					width = size.X;
				}
			}

			TooltipUI.DrawWidth = (int)width;
			TooltipUI.SetTooltip(tooltip);
			TooltipUI.SetName($"[c/{tooltipLines[0].Color.Hex3()}:{tooltipLines[0].Text}]");
		}
	}
}