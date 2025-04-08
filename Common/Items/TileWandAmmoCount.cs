using Terraria.GameContent;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.Items;

internal class TileWandAmmoCount : GlobalItem
{
	public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
	{
		if (item.tileWand >= 0)
		{
			int count = Main.LocalPlayer.CountItem(item.tileWand, 9999);
			position += new Vector2(6, 16);
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, count.ToString(), position, Color.White, 0f, origin, new(0.7f));
		}
	}
}
