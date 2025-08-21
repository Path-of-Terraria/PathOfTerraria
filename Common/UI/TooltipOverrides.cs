using MonoMod.Cil;
using PathOfTerraria.Common.Systems;

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


		Item hoverItem = Main.HoverItem;
		Item mouseItem = Main.mouseItem?.IsAir == false ? Main.mouseItem : player.inventory[58];
		bool drawForHoverItem = (hoverItem?.IsAir) == false && ShouldOverrideHoverItemTooltip();
		bool drawForMouseItem = (mouseItem?.IsAir) == false && Main.LocalPlayer.mouseInterface && !player.ItemAnimationActive && mouseItem.IsNotSameTypePrefixAndStack(hoverItem);
		bool drawSideBySide = drawForHoverItem && drawForMouseItem;

		if (drawForHoverItem)
		{
			Tooltip.Create(new TooltipDescription
			{
				Identifier = "HoverItem",
				AssociatedItem = hoverItem,
				Position = new Vector2(Main.mouseX + 18, Main.mouseY + 18),
				Origin = new Vector2(drawSideBySide ? 1f : 0f, 0f),
				Lines = ItemTooltipBuilder.BuildTooltips(hoverItem, Main.LocalPlayer),
				VisibilityTimeInTicks = 0,
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
				VisibilityTimeInTicks = 0,
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