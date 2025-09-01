using PathOfTerraria.Common.Config;
using PathOfTerraria.Common.Enums;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.ModPlayers.LivesSystem;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.UI.SmartUI;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.UI;

public class ExpBar : SmartUiState
{
	public override bool Visible => true;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Texture2D bar = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/BarEmpty").Value;
		Texture2D fill = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/BarFill").Value;

		ExpModPlayer mp = Main.LocalPlayer.GetModPlayer<ExpModPlayer>();

		var pos = new Vector2(Main.screenWidth / 2, 10);
		
		// Clamp the exp fill to 100 to prevent it from going infinitely off screen
		float fillPercentage = Math.Min(1f, mp.Exp / (float)mp.NextLevel);
		int fillWidth = (int)(fillPercentage * fill.Width);
		
		var target = new Rectangle((int)(pos.X - bar.Width / 2) + 6, (int)pos.Y + 14, fillWidth, fill.Height);
		var source = new Rectangle(0, 0, fillWidth, target.Height);

		spriteBatch.Draw(bar, pos, null, Color.White, 0, new Vector2(bar.Width / 2f, 0), 1, 0, 0);
		spriteBatch.Draw(fill, target, source, Color.White);

		Utils.DrawBorderString(spriteBatch, $"{mp.Level}", pos + new Vector2(bar.Width * 0.5f - 20, 22), Color.White, 0.8f, 0.5f, 0.5f);

		var bounding = new Rectangle((int)(pos.X - bar.Width / 2f), (int)pos.Y, bar.Width, bar.Height);

		if (bounding.Contains(Main.MouseScreen.ToPoint()))
		{
			double percent = Math.Truncate(mp.Exp / (float)mp.NextLevel * 10000) / 100f;
			// $"Level {mp.Level}\nExperience: {mp.Exp} / {mp.NextLevel} ({percent}%)\n\nClick to open skill tree"
			Utils.DrawBorderString(spriteBatch, Language.GetTextValue("Mods.PathOfTerraria.UI.ExpBarHover", mp.Level, mp.Exp, mp.NextLevel, percent), 
				Main.MouseScreen + Vector2.One * 24, Main.MouseTextColorReal);
		}

		string levelText = Language.GetTextValue("Mods.PathOfTerraria.UI.AreaLevel") + " " + PoTItemHelper.PickItemLevel();
		float halfWidth = ChatManager.GetStringSize(FontAssets.MouseText.Value, levelText, Vector2.One).X / 2f;
		Utils.DrawBorderString(spriteBatch, levelText, pos + new Vector2(-halfWidth - 10, 34), Color.White, 1);

		if (!Main.LocalPlayer.GetModPlayer<BossDomainLivesPlayer>().InDomain || SubworldSystem.Current is RavencrestSubworld)
		{
			return;
		}

		int lives = Main.LocalPlayer.GetModPlayer<BossDomainLivesPlayer>().GetLivesLeft();
		string text = lives + " " + Language.GetTextValue("Mods.PathOfTerraria.UI.Lives");
		halfWidth = ChatManager.GetStringSize(FontAssets.MouseText.Value, text, Vector2.One).X / 2f;
		Utils.DrawBorderString(spriteBatch, text, pos + new Vector2(-halfWidth - 10, 60), Color.White, 1);
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		Texture2D bar = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/BarEmpty").Value;
		var pos = new Vector2(Main.screenWidth / 2, 10);

		var bounding = new Rectangle((int)(pos.X - bar.Width / 2f), (int)pos.Y, bar.Width, bar.Height);

		if (!bounding.Contains(Main.MouseScreen.ToPoint()))
		{
			return;
		}

		if (!ModContent.GetInstance<UIConfig>().PreventExpBarClicking)
		{
			SmartUiLoader.GetUiState<TreeState>().Toggle();
		}
	}
}