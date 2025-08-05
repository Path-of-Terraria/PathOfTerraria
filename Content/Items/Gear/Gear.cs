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
		if (Item == Main.mouseItem || Item == player.inventory[58] && player.IsStandingStillForSpecialEffects && player.whoAmI == Main.myPlayer)
		{
			List<DrawableTooltipLine> tooltipLines = ItemTooltipBuilder.BuildTooltips(Item, player);
			TooltipUI.SetFancyTooltip(tooltipLines[1..]);
			TooltipUI.SetName($"[c/{tooltipLines[0].Color.Hex3()}:{tooltipLines[0].Text}]");
		}
	}

	/// <summary>
	/// Allows the modder to modify incoming lines added by Path of Terraria which may not be accessible in <see cref="ModItem.ModifyTooltips(List{TooltipLine})"/>.
	/// </summary>
	/// <param name="line">The line being added.</param>
	/// <returns>Whether the line will be added or not.</returns>
	public virtual bool ModifyNewTooltipLine(TooltipLine line)
	{
		return true;
	}
}