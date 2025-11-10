using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Core.Items;
using ReLogic.Content;
using ReLogic.Graphics;
using System.Linq;
using System.Reflection;

namespace PathOfTerraria.Common.UI;

internal class PopupTextModifications : ILoadable
{
	public static Asset<Texture2D> Icons = null;

	public static ItemRarity[] PopupTextRarity = [.. Enumerable.Repeat(ItemRarity.Invalid, 20)];

	public void Load(Mod mod)
	{
		IL_Main.DrawItemTextPopups += AddIcons;
		On_PopupText.NewText_AdvancedPopupRequest_Vector2 += UnsetRarity;
		On_PopupText.NewText_PopupTextContext_int_Vector2_bool += AlsoUnsetRarity;
		On_PopupText.NewText_PopupTextContext_Item_int_bool_bool += SetRarity;

		Icons = mod.Assets.Request<Texture2D>("Assets/UI/ItemRarityIcons");
	}

	private int SetRarity(On_PopupText.orig_NewText_PopupTextContext_Item_int_bool_bool orig, PopupTextContext context, Item newItem, int stack, bool noStack, bool longText)
	{
		int value = orig(context, newItem, stack, noStack, longText);

		if (value != -1)
		{
			if (newItem.TryGetGlobalItem(out PoTInstanceItemData data) && data.Rarity != ItemRarity.Normal)
			{
				PopupTextRarity[value] = data.Rarity;
			}
			else
			{
				PopupTextRarity[value] = ItemRarity.Invalid;
			}
		}

		return value;
	}

	private int AlsoUnsetRarity(On_PopupText.orig_NewText_PopupTextContext_int_Vector2_bool orig, PopupTextContext context, int npcNetID, Vector2 position, bool stay5TimesLonger)
	{
		int value = orig(context, npcNetID, position, stay5TimesLonger);

		if (value != -1)
		{
			PopupTextRarity[value] = ItemRarity.Invalid;
		}

		return value;
	}

	private int UnsetRarity(On_PopupText.orig_NewText_AdvancedPopupRequest_Vector2 orig, AdvancedPopupRequest request, Vector2 position)
	{
		int value = orig(request, position);

		if (value != -1)
		{
			PopupTextRarity[value] = ItemRarity.Invalid;
		}

		return value;
	}

	private void AddIcons(ILContext il)
	{
		static bool MatchDrawString(Instruction i)
		{
			return i.MatchCall(typeof(DynamicSpriteFontExtensionMethods), nameof(DynamicSpriteFontExtensionMethods.DrawString));
		}

		ILCursor c = new(il);

		// Match a part of the 'for (int i = 0; i < 20; i++)' loop to grab 'i'.
		int iLoc = -1;
		c.GotoNext(MoveType.After,
			i => i.MatchLdcI4(0),
			i => i.MatchStloc(out iLoc),
			i => i.MatchBr(out _)
		);
		// Match '((Color)(ref val6)).A = (byte)MathHelper.Lerp(60f, 127f, Utils.GetLerpValue(0f, 255f, num6, clamped: true));'.
		int secondColorLoc = -1;
		c.GotoNext(MoveType.After,
			i => i.MatchLdloca(out secondColorLoc),
			i => i.MatchLdcR4(60f),
			i => i.MatchLdcR4(127f),
			i => i.MatchLdcR4(0f),
			i => i.MatchLdcR4(255f)
		);

		// Navigate to the end.
		c.Index = c.Body.Instructions.Count - 1;
		// Go back to find the last DrawString call.
		c.GotoPrev(MoveType.Before, MatchDrawString);
		// Then find the closest earlier unconditional jump. This is a skip over the else case.
		ILLabel skipAllDrawsLabel = null;
		c.GotoPrev(MoveType.Before, i => i.MatchBr(out skipAllDrawsLabel));

		// Go back forward to the last DrawString call and hijack it.
		c.GotoNext(MoveType.Before, MatchDrawString);
		c.Emit(OpCodes.Ldloc_S, (byte)iLoc);
		c.EmitDelegate(HijackTextDraw);
		c.Emit(OpCodes.Br, skipAllDrawsLabel);

		// Reset index and go to the first DrawString call. Hijack both calls located there.
		c.Index = 0;
		c.GotoNext(MoveType.Before, MatchDrawString);
		c.Emit(OpCodes.Ldloc_S, (byte)secondColorLoc);
		c.Emit(OpCodes.Ldloc_S, (byte)iLoc);
		c.EmitDelegate(HijackTextDraw_Twin);
		c.Emit(OpCodes.Br, skipAllDrawsLabel);

		MonoModHooks.DumpIL(PoTMod.Instance, il);
	}

	// Both hijack methods take more parameters than they actually use to pop them off of the stack, DO NOT change this
	public static void HijackTextDraw(SpriteBatch batch, DynamicSpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, 
		SpriteEffects effects, float layer, int slot)
	{
		DrawText(batch, font, text, position, color, rotation, origin, scale, slot);
	}

	// Vanilla has an if that draws two texts for a reason I don't quite understand.
	// The if checks: color2 != Color.Black && j < 4
	// color2 is equivalent to secondColor here. I don't know the intent, or what context specifically this appears in, but this should keep parity.
	public static void HijackTextDraw_Twin(SpriteBatch batch, DynamicSpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale,
		SpriteEffects effects, float layer, Color secondColor, int slot)
	{
		DrawText(batch, font, text, position, Color.Lerp(color, secondColor, 0.5f), rotation, origin, scale, null);
		DrawText(batch, font, text, position, secondColor, rotation, origin, scale, slot);
	}

	public static void DrawText(SpriteBatch batch, DynamicSpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, int? slot)
	{
		bool hasRare = slot.HasValue && PopupTextRarity[slot.Value] != ItemRarity.Invalid;
		DynamicSpriteFontExtensionMethods.DrawString(batch, font, text, position, color, rotation, origin, new Vector2(scale), SpriteEffects.None, 0);

		if (hasRare)
		{
			var src = new Rectangle(20 * (int)PopupTextRarity[slot.Value], 0, 18, 18);
			Main.spriteBatch.Draw(Icons.Value, position, src, Color.White, rotation, origin + new Vector2(20, 0), scale, SpriteEffects.None, 0);
		}
	}

	public void Unload()
	{
	}
}
