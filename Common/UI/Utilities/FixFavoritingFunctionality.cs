using Mono.Cecil.Cil;
using MonoMod.Cil;
using PathOfTerraria.Utilities;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Utilities;

internal class FixFavoritingFunctionality : ILoadable
{
	/// <summary>
	/// If the player should be able to favorite equipped armor/accessories. Defaults to false to match vanilla.
	/// </summary>
	private static bool ArmorFavoritingEnabled => false;

	public void Load(Mod mod)
	{
		On_ItemSlot.OverrideHover_ItemArray_int_int += FixFavoriting;
		IL_ItemSlot.OverrideLeftClick += ModifyFavoritingFunctionality;
	}

	private void ModifyFavoritingFunctionality(ILContext il)
	{
		ILCursor c = new(il);

		// Anchor at ItemSlot.canFavoriteAt check
		if (!c.TryGotoNext(x => x.MatchLdsfld<ItemSlot>("canFavoriteAt")))
		{
			return;
		}

		int context = -1;

		// Match the argument indx of "context"
		if (!c.TryGotoNext(x => x.MatchLdarg(out context)))
		{
			return;
		}

		// Move after the bool value is placed on the stack approaching the brtrue that skips returning
		if (!c.TryGotoNext(MoveType.After, x => x.MatchLdelemU1()))
		{
			return;
		}

		// originalValue is already pushed, push context & additional check delegate
		c.Emit(OpCodes.Ldarg_S, (byte)context);
		c.EmitDelegate(HijackFavoriteValue);
	}

	private static bool HijackFavoriteValue(bool originalValue, int context)
	{
		// This method effectively changes the first line in this snippet:

		// if (!canFavoriteAt[Math.Abs(context)])
		//		return false;
		// item.favorited = !item.favorited;

		// into this:

		// if (!canFavoriteAt[Math.Abs(context)] && context >= 0)

		// This allows for us to use the negative context we have and still allow favoriting.

		if (!ArmorFavoritingEnabled)
		{
			return originalValue;
		}

		return originalValue || context < 0;
	}

	private void FixFavoriting(On_ItemSlot.orig_OverrideHover_ItemArray_int_int orig, Item[] inv, int context, int slot)
	{
		bool invalidFavorite = context < 0;

		// Define scope for override
		// This forces the code that checks for favoriting to fail as Keys 50,000 doesn't exist, stopping the OOB check on ItemSlot.canFavoriteSlot
		{
			using var _ = ValueOverride.Create(ref Main.FavoriteKey, invalidFavorite ? (Microsoft.Xna.Framework.Input.Keys)50000 : Main.FavoriteKey);
			orig(inv, context, slot);
		}

		if (!invalidFavorite || !ArmorFavoritingEnabled)
		{
			return;
		}

		Item item = inv[slot];

		if (Main.keyState.IsKeyDown(Main.FavoriteKey))
		{
			if (item.type > ItemID.None && item.stack > 0 && Main.drawingPlayerChat)
			{
				Main.cursorOverride = 2;
			}
			else if (item.type > ItemID.None && item.stack > 0)
			{
				Main.cursorOverride = 3;
			}
		}
	}

	public void Unload()
	{
	}
}
