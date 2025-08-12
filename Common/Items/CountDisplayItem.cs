using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.Items;

internal class CountDisplayItem : GlobalItem
{
	public static int Context = -1;
	public static int ForcedContext = -1;

	public override void Load()
	{
		On_ItemSlot.Draw_SpriteBatch_ItemArray_int_int_Vector2_Color += GetContext;
	}

	private void GetContext(On_ItemSlot.orig_Draw_SpriteBatch_ItemArray_int_int_Vector2_Color orig, SpriteBatch spriteBatch, Item[] inv, int context, int slot, Vector2 position, 
		Color lightColor)
	{
		Context = context;
		orig(spriteBatch, inv, context, slot, position, lightColor);
		Context = -1;
	}

	public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		int context = Context;

		if (ForcedContext != -1)
		{
			context = ForcedContext;
		}

		if (context is not ItemSlot.Context.InventoryItem and not ItemSlot.Context.HotbarItem)
		{
			return;
		}

		if (item.tileWand >= 0)
		{
			int count = Main.LocalPlayer.CountItem(item.tileWand, 9999);
			position += new Vector2(6, 16);
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, count.ToString(), position, Color.White, 0f, origin, new(0.7f));
		}

		if (item.useAmmo > AmmoID.None)
		{
			int count = 0;

			_ = item.useAmmo;

			for (int j = 0; j < Main.InventorySlotsTotal; j++)
			{
				Item invItem = Main.LocalPlayer.inventory[j];

				if (invItem.stack > 0 && ItemLoader.CanChooseAmmo(item, invItem, Main.LocalPlayer))
				{
					count += invItem.stack;
				}
			}

			position += new Vector2(-2, 24);
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, count.ToString(), position, Color.White, 0f, origin, new(0.7f));
		}
	}
}
