using FunnyExperience.Core.Loaders.UILoading;
using FunnyExperience.Core.Systems;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.UI;

namespace FunnyExperience.Content.GUI
{
	internal class NewHotbar : SmartUIState
	{
		private int _animation;

		private float _selectorX;
		private float _selectorTarget;

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
				if (_animation > 0)
					_animation--;

				prog = 1 - Ease((20 - _animation) / 20f);
			}
			else
			{
				if (_animation < 20)
					_animation++;

				prog = Ease(_animation / 20f);
			}

			if (Main.LocalPlayer.selectedItem >= 2)
				_selectorTarget = 24 + 120 + 52 * (Main.LocalPlayer.selectedItem - 2);
			else
				_selectorTarget = 98;

			_selectorX += (_selectorTarget - _selectorX) * 0.33f;

			DrawCombat(spriteBatch, -prog * 80, 1 - prog);
			DrawBuilding(spriteBatch, 80 - prog * 80, prog);
		}

		public void DrawCombat(SpriteBatch spriteBatch, float off, float opacity)
		{
			Texture2D combat = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/HotbarCombat").Value;
			Main.inventoryScale = 36 / 52f * 52f / 36f * opacity;

			Main.spriteBatch.Draw(combat, new Vector2(20, 20 + off), null, Color.White * opacity);
			ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[0], 21, new Vector2(24, 30 + off));

			PotionSystem potionPlayer = Main.LocalPlayer.GetModPlayer<PotionSystem>();

			Texture2D bottleTex = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/EmptyPotion").Value;
			Texture2D lifeTexture = Terraria.GameContent.TextureAssets.Item[ItemID.LesserHealingPotion].Value;
			Texture2D manaTexture = Terraria.GameContent.TextureAssets.Item[ItemID.LesserManaPotion].Value;

			spriteBatch.Draw(bottleTex, new Vector2(471, 40 + off), Color.White * opacity);
			spriteBatch.Draw(bottleTex, new Vector2(523, 40 + off), Color.White * opacity);

			int lifeH = (int)(potionPlayer.HealingLeft / (float)potionPlayer.MaxHealing * lifeTexture.Height);
			int manaH = (int)(potionPlayer.ManaLeft / (float)potionPlayer.MaxMana * lifeTexture.Height);

			spriteBatch.Draw(lifeTexture, new Rectangle(471, (int)(40 + off) + lifeTexture.Height - lifeH, bottleTex.Width, lifeH), new Rectangle(0, lifeTexture.Height - lifeH, lifeTexture.Width, lifeH), Color.White * opacity);
			spriteBatch.Draw(manaTexture, new Rectangle(523, (int)(40 + off) + manaTexture.Height - manaH, bottleTex.Width, manaH), new Rectangle(0, manaTexture.Height - manaH, manaTexture.Width, manaH), Color.White * opacity);

			Utils.DrawBorderString(spriteBatch, $"{potionPlayer.HealingLeft}/{potionPlayer.MaxHealing}", new Vector2(480, 112 + off), (potionPlayer.HealingLeft > 0 ? new Color(255, 200, 200) : Color.Gray) * opacity, 1f * opacity, 0.5f, 0.5f);
			Utils.DrawBorderString(spriteBatch, $"{potionPlayer.ManaLeft}/{potionPlayer.MaxMana}", new Vector2(534, 112 + off), (potionPlayer.ManaLeft > 0 ? new Color(200, 220, 255) : Color.Gray) * opacity, 1f * opacity, 0.5f, 0.5f);

			if (Main.LocalPlayer.HasBuff(BuffID.PotionSickness))
			{
				Texture2D glow = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/GlowSoft").Value;
				spriteBatch.Draw(glow, new Vector2(480, 60 + off), null, Color.Black, 0, glow.Size() / 2f, 1, 0, 0);
				Utils.DrawBorderString(spriteBatch, $"{Main.LocalPlayer.buffTime[Main.LocalPlayer.FindBuffIndex(BuffID.PotionSickness)] / 60 + 1}", new Vector2(480, 60 + off), Color.LightGray * opacity, 1f * opacity, 0.5f, 0.5f);
			}

			if (Main.LocalPlayer.HasBuff(BuffID.ManaSickness))
			{
				Texture2D glow = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/GlowSoft").Value;
				spriteBatch.Draw(glow, new Vector2(534, 60 + off), null, Color.Black, 0, glow.Size() / 2f, 1, 0, 0);
				Utils.DrawBorderString(spriteBatch, $"{Main.LocalPlayer.buffTime[Main.LocalPlayer.FindBuffIndex(BuffID.ManaSickness)] / 60 + 1}", new Vector2(534, 60 + off), Color.LightGray * opacity, 1f * opacity, 0.5f, 0.5f);
			}
		}

		public void DrawBuilding(SpriteBatch spriteBatch, float off, float opacity)
		{
			Texture2D building = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/HotbarBuilding").Value;
			Main.inventoryScale = 36 / 52f * 52f / 36f * opacity;

			Main.spriteBatch.Draw(building, new Vector2(20, 20 + off), null, Color.White * opacity);
			ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[1], 21, new Vector2(24 + 62, 30 + off));

			for (int k = 2; k <= 9; k++)
			{
				ItemSlot.Draw(spriteBatch, ref Main.LocalPlayer.inventory[k], 21, new Vector2(24 + 124 + 52 * (k - 2), 30 + off));
			}

			Texture2D select = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/HotbarSelector").Value;
			Main.spriteBatch.Draw(select, new Vector2(_selectorX, 21 + off), null, Color.White * opacity * (_selectorTarget == 98 ? (_selectorX - 98) / 30f : 1));
		}

		public float Ease(float input)
		{
			float c1 = 1.70158f;
			float c3 = c1 + 1;

			return 1 + c3 * (float)Math.Pow(input - 1, 3) + c1 * (float)Math.Pow(input - 1, 2);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			Texture2D bar = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/BarEmpty").Value;
			var pos = new Vector2(Main.screenWidth / 2, 10);

			var bounding = new Rectangle((int)(pos.X - bar.Width / 2f), (int)pos.Y, bar.Width, bar.Height);

			if (bounding.Contains(Main.MouseScreen.ToPoint()))
				UILoader.GetUIState<Tree>().visible = true;
		}
	}
}
