using PathOfTerraria.Common.Config;
using PathOfTerraria.Common.Subworlds;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.ModPlayers.LivesSystem;
using PathOfTerraria.Core.Items;
using PathOfTerraria.Core.UI.SmartUI;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.UI;

public class ExpBar : SmartUiState
{
	private const int ExperienceGainDisplayTime = 180;

	private int _experienceGainTimer;
	private int _recentExperienceGained;

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
		DrawExperienceGain(spriteBatch, pos, bar.Width);

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

	public void AddExperienceGain(int experience)
	{
		if (experience <= 0)
		{
			return;
		}

		_recentExperienceGained = (int)Math.Min(int.MaxValue, (long)_recentExperienceGained + experience);
		_experienceGainTimer = ExperienceGainDisplayTime;
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		if (_experienceGainTimer > 0)
		{
			_experienceGainTimer--;
		}
		else
		{
			_recentExperienceGained = 0;
		}
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

	private void DrawExperienceGain(SpriteBatch spriteBatch, Vector2 barPosition, int barWidth)
	{
		if (_experienceGainTimer <= 0 || _recentExperienceGained <= 0)
		{
			return;
		}

		float remaining = _experienceGainTimer / (float)ExperienceGainDisplayTime;
		float elapsed = 1f - remaining;
		float fadeIn = Utils.GetLerpValue(0f, 0.12f, elapsed, true);
		float fadeOut = Utils.GetLerpValue(0f, 0.25f, remaining, true);
		float opacity = fadeIn * fadeOut;
		float pop = Utils.GetLerpValue(0.22f, 0f, elapsed, true);
		float scale = 0.85f + pop * 0.25f;
		string text = Language.GetTextValue("Mods.PathOfTerraria.UI.ExpGain", _recentExperienceGained);
		Vector2 position = barPosition + new Vector2(barWidth / 2f + 22, 28 - 8f * elapsed);

		Utils.DrawBorderString(spriteBatch, text, position, new Color(255, 215, 95) * opacity, scale, 0f, 0.5f);
	}
}
