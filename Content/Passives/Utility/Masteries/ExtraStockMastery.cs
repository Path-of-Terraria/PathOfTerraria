using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using Terraria.GameContent;

namespace PathOfTerraria.Content.Passives.Utility.Masteries;

internal class ExtraStockMastery : Passive
{
	internal class ExtraStockPlayer : ModPlayer
	{
		public override void Load()
		{
			On_Main.DrawInventory += DrawInventory_RerollButton;
		}

		private void DrawInventory_RerollButton(On_Main.orig_DrawInventory orig, Main self)
		{
			orig(self);

			const int num7 = 10;
			const int num8 = 1;

			int x = (int)(73f + (float)(num7 * 56) * Main.inventoryScale);
			int y = (int)((float)Main.instance.invBottom + (float)(num8 * 56) * Main.inventoryScale);

			Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Vector2(x, y), new Rectangle(0, 0, 24, 24), Color.White);
		}
	}
}
