using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Common.ModCompatibility;
using PathOfTerraria.Common.UI;
using PathOfTerraria.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.Systems.VanillaModifications;

/// <summary>
/// Used to fix tooltip backing panel rendering incorrectly. Why isn't this in tMod already?
/// </summary>
internal class TooltipBackingFix : ILoadable
{
	public void Load(Mod mod)
	{
		IL_Main.MouseText_DrawItemTooltip += ModifyBackingSize;
	}

	private void ModifyBackingSize(ILContext il)
	{
		ILCursor c = new(il);

		if (!c.TryGotoNext(x => x.MatchCall(typeof(Utils).FullName, nameof(Utils.DrawInvBG))))
		{
			return;
		}

		if (!c.TryGotoPrev(x => x.MatchLdsfld<Main>(nameof(Main.spriteBatch))))
		{
			return;
		}

		// Locals are resolved by type/usage rather than hardcoded indices, as tML updates shift local slots.
		// The size local is the Vector2 whose X is read when building the DrawInvBG rectangle.
		int sizeLoc = -1;

		if (!c.Clone().TryGotoNext(x => x.MatchLdloc(out sizeLoc), x => x.MatchLdfld<Vector2>(nameof(Vector2.X))))
		{
			PoTMod.Instance.Logger.Error("IL edit TooltipBackingFix failed: couldn't find tooltip size local.");
			return;
		}

		VariableDefinition itemLoc = FindSingleLocal(il, x => x.FullName == typeof(Item).FullName);
		VariableDefinition linesLoc = FindSingleLocal(il, x => x is GenericInstanceType g
			&& g.ElementType.FullName == typeof(List<>).FullName && g.GenericArguments[0].FullName == typeof(DrawableTooltipLine).FullName);

		if (itemLoc is null || linesLoc is null)
		{
			PoTMod.Instance.Logger.Error("IL edit TooltipBackingFix failed: couldn't find item or tooltip line locals.");
			return;
		}

		c.Emit(OpCodes.Ldloc, itemLoc);
		c.Emit(OpCodes.Ldloc, linesLoc);
		c.Emit(OpCodes.Ldloca, il.Body.Variables[sizeLoc]);

		c.EmitDelegate(ActuallyModifyBack);
	}

	private static VariableDefinition FindSingleLocal(ILContext il, Func<TypeReference, bool> predicate)
	{
		VariableDefinition[] matches = il.Body.Variables.Where(x => predicate(x.VariableType)).ToArray();
		return matches.Length == 1 ? matches[0] : null;
	}

	public static void ActuallyModifyBack(Item item, List<DrawableTooltipLine> tooltips, ref Vector2 size)
	{
		float maxWidth = 0;

		using var _ = ValueOverride.Create(ref Tooltip.SuppressDrawing, true);

		foreach (DrawableTooltipLine tooltip in tooltips)
		{
			int yOffset = 0; // Used for getting real height
			Vector2 oldScale = tooltip.BaseScale;

			// Skip tooltips that won't draw
			if (!ItemLoader.PreDrawTooltipLine(item, tooltip, ref yOffset))
			{
				continue;
			}

			// Measure the line, set maxWidth, then reset scale in case it'd cause issues later
			Vector2 lineSize = ChatManager.GetStringSize(tooltip.Font, tooltip.Text, Vector2.One) * tooltip.BaseScale;
			maxWidth = MathF.Max(maxWidth, lineSize.X);
			tooltip.BaseScale = oldScale;
		}

		size.X = maxWidth; // Max width is accurately calculated by the above
		size.Y -= tooltips.Count * 1.2f; // Max height isn't accurately calculated by the above for whatever reason, use bandaid
	}

	public void Unload()
	{
	}
}
