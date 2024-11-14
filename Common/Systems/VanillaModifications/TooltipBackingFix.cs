using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
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

		c.Emit(OpCodes.Ldloc_S, (byte)2);
		c.Emit(OpCodes.Ldloc_S, (byte)20);
		c.Emit(OpCodes.Ldloca_S, (byte)17);

		c.EmitDelegate(ActuallyModifyBack);
	}

	public static void ActuallyModifyBack(Item item, List<DrawableTooltipLine> tooltips, ref Vector2 size)
	{
		float maxWidth = 0;
		float maxHeight = 0;

		foreach (DrawableTooltipLine tooltip in tooltips)
		{
			int yOffset = 0; // Used for getting real height
			Vector2 oldScale = tooltip.BaseScale;
			ItemLoader.PreDrawTooltipLine(item, tooltip, ref yOffset);

			// Measure the line, set maxWidth, and add to height, then reset scale in case it'd cause issues later
			Vector2 lineSize = ChatManager.GetStringSize(tooltip.Font, tooltip.Text, Vector2.One) * tooltip.BaseScale;
			maxWidth = MathF.Max(maxWidth, lineSize.X);
			maxHeight += lineSize.Y + yOffset;
			tooltip.BaseScale = oldScale;
		}

		size.X = maxWidth;
		size.Y = maxHeight + 30;
	}

	public void Unload()
	{
	}
}
