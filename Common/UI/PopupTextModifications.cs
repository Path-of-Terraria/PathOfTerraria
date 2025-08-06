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
		ILCursor c = new(il);

		MethodInfo drawStringInfo = typeof(DynamicSpriteFontExtensionMethods).GetMethod(nameof(DynamicSpriteFontExtensionMethods.DrawString), 
			BindingFlags.Public | BindingFlags.Static,
			[typeof(SpriteBatch), typeof(DynamicSpriteFont), typeof(string), typeof(Vector2), typeof(Color), typeof(float), typeof(Vector2), typeof(float),
			typeof(SpriteEffects), typeof(float)]);

		if (!c.TryGotoNext(MoveType.After, x => x.MatchCall(drawStringInfo)))
		{
			return;
		}

		ILLabel skipLabel = null;

		if (!c.TryGotoNext(x => x.MatchBr(out skipLabel)))
		{
			return;
		}

		if (!c.TryGotoPrev(x => x.MatchCall(drawStringInfo)))
		{
			return;
		}

		c.Emit(OpCodes.Ldloc_S, (byte)21);
		c.Emit(OpCodes.Ldloc_0);
		c.EmitDelegate(HijackTextDraw_Twin);
		c.Emit(OpCodes.Br, skipLabel);

		if (!c.TryGotoNext(x => x.MatchCall(drawStringInfo)))
		{
			return;
		}

		if (!c.TryGotoNext(x => x.MatchCall(drawStringInfo)))
		{
			return;
		}

		c.Emit(OpCodes.Ldloc_0);
		c.EmitDelegate(HijackTextDraw);
		c.Emit(OpCodes.Br, skipLabel);
	}

	// Both hijack methods take more parameters than they actually use to pop them off of the stack, DO NOT change this

	public static void HijackTextDraw(SpriteBatch batch, DynamicSpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, 
		SpriteEffects effects, float layer, int slot)
	{
		DrawText(batch, font, text, position, color, rotation, origin, scale, slot);
	}

	public static void HijackTextDraw_Twin(SpriteBatch batch, DynamicSpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale,
		SpriteEffects effects, float layer, Color secondColor, int slot)
	{
		DrawText(batch, font, text, position, Color.Lerp(color, secondColor, 0.5f), rotation, origin, scale, null);
		DrawText(batch, font, text, position, secondColor, rotation, origin, scale, slot);
	}

	public static void DrawText(SpriteBatch batch, DynamicSpriteFont font, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, int? slot)
	{
		DynamicSpriteFontExtensionMethods.DrawString(batch, font, text, position, color, rotation, origin, new Vector2(scale), SpriteEffects.None, 0);

		if (slot.HasValue && PopupTextRarity[slot.Value] != ItemRarity.Invalid)
		{
			var src = new Rectangle(20 * (int)PopupTextRarity[slot.Value], 0, 18, 18);
			Main.spriteBatch.Draw(Icons.Value, position, src, Color.White, rotation, origin + new Vector2(20, 0), scale, SpriteEffects.None, 0);
		}
	}

	public void Unload()
	{
	}
}
