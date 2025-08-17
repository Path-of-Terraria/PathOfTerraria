using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using MonoMod.Cil;
using PathOfTerraria.Common.Systems;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.UI;

#nullable enable

/// <summary>
/// System that overrides various vanilla tooltip rendering with the <see cref="Tooltip"/>'s API.
/// </summary>
public sealed class TooltipOverrides : ModSystem
{
	public override void Load()
	{
		IL_Main.MouseTextInner += InjectHoverItemTooltipOverride;
		On_Main.DrawInterface_33_MouseText += OnDrawMouseText;
	}

	private static void OnDrawMouseText(On_Main.orig_DrawInterface_33_MouseText orig, Main self)
	{
		AddDefaultTooltips();
	}

	private static bool ShouldOverrideHoverItemTooltip()
	{
		return true;
	}

	private static void AddDefaultTooltips()
	{
		// Create default tooltips to override the game's with ours.
		Player player = Main.LocalPlayer;

		bool drawingHoverItem = false;
		Item hoverItem = Main.HoverItem;
		if ((hoverItem?.IsAir) == false && !Main.mouseText && ShouldOverrideHoverItemTooltip())
		{
			drawingHoverItem = true;
			Tooltip.Create(new TooltipDescription
			{
				Identifier = "HoverItem",
				AssociatedItem = hoverItem,
				Position = new Vector2(Main.mouseX + 34, Main.mouseY + 34),
				Lines = ItemTooltipBuilder.BuildTooltips(hoverItem, Main.LocalPlayer),
			});
		}

		Item mouseItem = Main.mouseItem?.IsAir == false ? Main.mouseItem : player.inventory[58];
		if ((mouseItem?.IsAir) == false && player.IsStandingStillForSpecialEffects && !player.ItemAnimationActive && !drawingHoverItem)
		{
			Tooltip.Create(new TooltipDescription
			{
				Identifier = "MouseItem",
				AssociatedItem = mouseItem,
				Position = new Vector2(Main.mouseX + 34, Main.mouseY + 34),
				Lines = ItemTooltipBuilder.BuildTooltips(mouseItem, Main.LocalPlayer),
			});
		}
	}

	private static void InjectHoverItemTooltipOverride(ILContext ctx)
	{
		var il = new ILCursor(ctx);

		// Collect local variable information.
		int locX = -1;
		int locY = -1;
		int locRare = -1;
		int locDiff = -1;
		Type cacheType = typeof(Main).GetNestedType("MouseTextCache", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!;
		il.FindNext(out _, i => i.MatchLdfld(cacheType, "rare"), i => i.MatchStloc(out locRare));
		il.FindNext(out _, i => i.MatchLdfld(cacheType, "diff"), i => i.MatchStloc(out locDiff));
		//il.FindNext(out _, i => i.MatchLdfld(cacheType, "X"), i => i.MatchStloc(out locX));
		//il.FindNext(out _, i => i.MatchLdfld(cacheType, "Y"), i => i.MatchStloc(out locY));
		il.FindNext(out _,
			i => i.MatchLdsfld(typeof(Main), nameof(Main.mouseX)),
			i => i.MatchLdcI4(14),
			i => i.MatchAdd(),
			i => i.MatchStloc(out locX)
		);
		il.FindNext(out _,
			i => i.MatchLdsfld(typeof(Main), nameof(Main.mouseY)),
			i => i.MatchLdcI4(14),
			i => i.MatchAdd(),
			i => i.MatchStloc(out locY)
		);

		// Match 'if (HoverItem.type > 0)'.
		il.GotoNext(
			MoveType.After,
			i => i.MatchLdsfld(typeof(Main), nameof(Main.HoverItem)),
			i => i.MatchLdfld(typeof(Item), nameof(Item.type)),
			i => i.MatchLdcI4(0),
			i => i.MatchBle(out _)
		);

		// Emit our condition.
		{
			// if (ShouldRenderMouseItemTooltip) {
			ILLabel skipNewReturnLabel = il.DefineLabel();
			il.EmitDelegate(ShouldOverrideHoverItemTooltip);
			il.EmitBrfalse(skipNewReturnLabel);
			// return;
			il.EmitRet();
			il.MarkLabel(skipNewReturnLabel);
		}
	}
}