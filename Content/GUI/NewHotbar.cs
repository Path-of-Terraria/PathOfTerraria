using FunnyExperience.Core.Loaders.UILoading;
using System.Collections.Generic;
using Terraria.UI;

namespace FunnyExperience.Content.GUI
{
	internal class NewHotbar : SmartUIState
	{
		public int animation;

		public override bool Visible => !Main.playerInventory;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var hideTarget = new Rectangle(20, 20, 446, 52);

			if (!Main.screenTarget.IsDisposed)
				Main.spriteBatch.Draw(Main.screenTarget, hideTarget, hideTarget, Color.White);

			float prog;

			if (Main.LocalPlayer.selectedItem == 0)
			{
				if (animation > 0)
					animation--;

				prog = 1 - Ease((20 - animation) / 20f);
			}
			else
			{
				if (animation < 20)
					animation++;

				prog = Ease(animation / 20f);
			}

			DrawCombat(spriteBatch, -prog * 80, 1 - prog);
			DrawBuilding(spriteBatch, 80 - prog * 80, prog);
		}

		public void DrawCombat(SpriteBatch spriteBatch, float off, float opacity)
		{
			Texture2D combat = ModContent.Request<Texture2D>("FunnyExperience/Assets/HotbarCombat").Value;
			Main.inventoryScale = 36 / 52f * 52f / 36f * opacity;

			Main.spriteBatch.Draw(combat, new Vector2(20, 20 + off), null, Color.White * opacity);
			ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[0], 21, new Vector2(24, 30 + off));
		}

		public void DrawBuilding(SpriteBatch spriteBatch, float off, float opacity)
		{
			Texture2D building = ModContent.Request<Texture2D>("FunnyExperience/Assets/HotbarBuilding").Value;
			Main.inventoryScale = 36 / 52f * 52f / 36f * opacity;

			Main.spriteBatch.Draw(building, new Vector2(20, 20 + off), null, Color.White * opacity);
			ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[1], 21, new Vector2(24 + 62, 30 + off));

			for (int k = 2; k <= 9; k++)
			{
				ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[k], 21, new Vector2(24 + 124 + 52 * (k - 2), 30 + off));
			}
		}

		public float Ease(float input)
		{
			float c1 = 1.70158f;
			float c3 = c1 + 1;

			return 1 + c3 * (float)Math.Pow(input - 1, 3) + c1 * (float)Math.Pow(input - 1, 2);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			Texture2D bar = ModContent.Request<Texture2D>("FunnyExperience/Assets/BarEmpty").Value;
			var pos = new Vector2(Main.screenWidth / 2, 10);

			var bounding = new Rectangle((int)(pos.X - bar.Width / 2f), (int)pos.Y, bar.Width, bar.Height);

			if (bounding.Contains(Main.MouseScreen.ToPoint()))
				UILoader.GetUIState<Tree>().visible = true;
		}
	}
}
