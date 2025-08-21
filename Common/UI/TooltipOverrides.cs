using MonoMod.Cil;
using PathOfTerraria.Common.Systems;

namespace PathOfTerraria.Common.UI;

#nullable enable

/// <summary>
/// System that overrides various vanilla tooltip rendering with the <see cref="Tooltip"/>'s API.
/// </summary>
public sealed class TooltipOverrides : ModSystem
{
	private static bool drewHoverItem;
	private static bool drewMouseItem;

	public override void Load()
	{
		IL_Main.MouseTextInner += InjectHoverItemTooltipOverride;

		On_Main.DrawInterface_34_PlayerChat += static (orig, self) =>
		{
			orig(self);
			AddDefaultTooltips(isLate: false);
		};
		On_Main.DrawInterface_41_InterfaceLogic4 += static (orig) =>
		{
			orig();
			AddDefaultTooltips(isLate: true);
		};
	}

	private static bool ShouldOverrideHoverItemTooltip()
	{
		return true;
	}

	private static void AddDefaultTooltips(bool isLate)
	{
		// Create default tooltips to override the game's with ours.
		Player player = Main.LocalPlayer;

		Item hoverItem = Main.HoverItem;
		Item mouseItem = Main.mouseItem?.IsAir == false ? Main.mouseItem : player.inventory[58];
		bool drawForHoverItem = (!isLate || !drewHoverItem) && (hoverItem?.IsAir) == false && ShouldOverrideHoverItemTooltip();
		bool isHoveringOverUI = Main.LocalPlayer.mouseInterface || drawForHoverItem || drewHoverItem;
		bool drawForMouseItem = (!isLate || !drewMouseItem) && (mouseItem?.IsAir) == false && isHoveringOverUI && !player.ItemAnimationActive && mouseItem.IsNotSameTypePrefixAndStack(hoverItem);
		bool drawSideBySide = drawForHoverItem && drawForMouseItem;

		// If we are reacting to a late Hover/MouseItem mutation, i.e. after the tooltips have already been drawn, then we need to make new instances last a whole tick.
		uint visibilityTimeInTicks = isLate ? 1u : 0u;
		drewHoverItem = drawForHoverItem || (isLate && drewHoverItem);
		drewMouseItem = drawForMouseItem || (isLate && drewHoverItem);

		if (drawForHoverItem)
		{
			Tooltip.Create(new TooltipDescription
			{
				Identifier = "HoverItem",
				AssociatedItem = hoverItem,
				Position = new Vector2(Main.mouseX + 18, Main.mouseY + 18),
				Origin = new Vector2(drawSideBySide ? 1f : 0f, 0f),
				Lines = ItemTooltipBuilder.BuildTooltips(hoverItem, Main.LocalPlayer),
				VisibilityTimeInTicks = visibilityTimeInTicks,
			});
		}

		if (drawForMouseItem)
		{
			Tooltip.Create(new TooltipDescription
			{
				Identifier = "MouseItem",
				AssociatedItem = mouseItem,
				Position = new Vector2(Main.mouseX + 18, Main.mouseY + 18),
				Lines = ItemTooltipBuilder.BuildTooltips(mouseItem, Main.LocalPlayer),
				VisibilityTimeInTicks = visibilityTimeInTicks,
			});
		}
	}

	private static void InjectHoverItemTooltipOverride(ILContext ctx)
	{
		var il = new ILCursor(ctx);

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