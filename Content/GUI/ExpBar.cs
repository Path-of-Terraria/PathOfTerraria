using FunnyExperience.Core.Loaders.UILoading;
using System.Collections.Generic;
using Terraria.UI;

namespace FunnyExperience.Content.GUI
{
	public class ExpBar : SmartUIState
	{
		public override bool Visible => true;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Texture2D bar = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/BarEmpty").Value;
			Texture2D fill = ModContent.Request<Texture2D>($"{FunnyExperience.ModName}/Assets/BarFill").Value;

			Core.Systems.ExpSystem mp = Main.LocalPlayer.GetModPlayer<Core.Systems.ExpSystem>();

			var pos = new Vector2(Main.screenWidth / 2, 10);
			var target = new Rectangle((int)(pos.X - bar.Width / 2) + 6, (int)pos.Y + 14, (int)(mp.exp / (float)mp.NextLevel * fill.Width), fill.Height);
			var source = new Rectangle(0, 0, target.Width, target.Height);

			spriteBatch.Draw(bar, pos, null, Color.White, 0, new Vector2(bar.Width / 2f, 0), 1, 0, 0);
			spriteBatch.Draw(fill, target, source, Color.White);

			Utils.DrawBorderString(spriteBatch, $"{mp.Level}", pos + new Vector2(bar.Width * 0.5f - 20, 22), Color.White, 0.8f, 0.5f, 0.5f);

			var bounding = new Rectangle((int)(pos.X - bar.Width / 2f), (int)pos.Y, bar.Width, bar.Height);

			if (bounding.Contains(Main.MouseScreen.ToPoint()))
				Utils.DrawBorderString(spriteBatch, $"Level {mp.Level}\nExperience: {mp.exp} / {mp.NextLevel} ({Math.Truncate(mp.exp / (float)mp.NextLevel * 10000) / 100f}%)\n\nClick to open skill tree", Main.MouseScreen + Vector2.One * 24, Main.MouseTextColorReal);
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