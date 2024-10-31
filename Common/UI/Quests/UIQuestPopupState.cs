using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Graphics;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.UI.Quests;

internal class UIQuestPopupState : SmartUiState
{
	/// <summary>
	/// Defines a text to popup on the screen, fade in and fade out.
	/// </summary>
	/// <param name="text">Text to show.</param>
	/// <param name="time">Amount of time for the text to display.</param>
	/// <param name="scale">Scale for the text to [begin] display at.</param>
	/// <param name="endScale">Scale for the text to end on. If -1, will be set to <paramref name="scale"/>.</param>
	public class PopupText(LocalizedText text, int time, float scale, float endScale = -1)
	{
		public int MaxTime = time;
		public int Time = time;
		public float Opacity = 0;
		public LocalizedText Text = text;
		public float Scale = scale;
		public float StartScale = scale;
		public float EndScale = endScale == -1 ? scale : endScale;

		public void DrawAndUpdate(Vector2 position)
		{
			if (Time <= 0)
			{
				return;
			}
			
			// Always draws centered, with color based on scale (lerp from black to white)
			DynamicSpriteFont font = FontAssets.DeathText.Value;
			Vector2 size = ChatManager.GetStringSize(font, Text.Value, Vector2.One);
			Color color = Color.Lerp(Color.Black, Color.White, (MathHelper.Clamp(Scale, 0.5f, 1.5f) - 0.5f) / 1f) * Opacity;
			ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, Text.Value, position, color, 0f, size / 2f, new(Scale));

			Time--;
			Scale = MathHelper.Lerp(StartScale, EndScale, 1 - Time / (float)MaxTime);

			if (Time < 60)
			{
				Opacity = Time / 60f;
			}
			else
			{
				Opacity = MathHelper.Lerp(Opacity, 1, 0.05f);
			}
		}
	}

	/// <summary>
	/// The new quest the player just unlocked. Set in <see cref="QuestModPlayer.StartQuest(string, int, bool)"/>.
	/// </summary>
	public static PopupText NewQuest = new(LocalizedText.Empty, 0, 1f);

	/// <summary>
	/// Displays the "New Quest:" text above the <see cref="NewQuest"/> text.
	/// </summary>
	public static PopupText NewQuestSupertext = new(Language.GetText("Mods.PathOfTerraria.Quests.Popups.NewQuest"), 0, 0.7f);

	/// <summary>
	/// Controls the tutorial prompt for the Quest Book & the Quest Book flashing.
	/// </summary>
	public static int FlashQuestButton = 0;

	/// <summary>
	/// The opacity for the tutorial prompt for the Quest Book (see <see cref="FlashQuestButton"/>).
	/// </summary>
	public static float ExplainOpacity = 0;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public override void OnInitialize()
	{
		// This UI is always visible, just usually not displaying anything.
		Visible = true;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);

		// Only run this method if there's something to display
		if (FlashQuestButton < 0 && NewQuest.Time < 0)
		{
			return;
		}

		FlashQuestButton--;

		if (SmartUiLoader.GetUiState<QuestsUIState>().IsVisible) // Hide flashing quest button & popup if in quest book
		{
			FlashQuestButton = -1;
		}

		Vector2 newQuestPos = Main.ScreenSize.ToVector2() / new Vector2(2, 1.25f);

		if (NewQuest.Time > 0)
		{
			Texture2D tex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/QuestNameBack").Value;
			float scale = MathHelper.Lerp(NewQuest.Scale, 1, 0.5f);

			Main.spriteBatch.Draw(tex, newQuestPos - new Vector2(0, 40), null, Color.White * NewQuest.Opacity, 0f, tex.Size() / 2f, scale, SpriteEffects.None, 0);
		}

		NewQuestSupertext.Time = NewQuest.Time;
		NewQuestSupertext.MaxTime = NewQuest.MaxTime;
		NewQuestSupertext.StartScale = 0.7f;
		NewQuestSupertext.EndScale = 0.9f;
		NewQuestSupertext.DrawAndUpdate(newQuestPos - new Vector2(0, 60));
		NewQuest.DrawAndUpdate(newQuestPos);

		ExplainOpacity = MathHelper.Lerp(ExplainOpacity, FlashQuestButton > 0 ? 1f : 0, 0.03f);

		if (ExplainOpacity > 0)
		{
			Texture2D tex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/QuestPromptBack").Value;
			Vector2 textPos = Main.ScreenSize.ToVector2() / new Vector2(2, 4);

			Main.spriteBatch.Draw(tex, textPos, null, Color.White * ExplainOpacity, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0);

			DrawCenteredExplanationText(Language.GetTextValue($"Mods.{PoTMod.ModName}.Quests.Popups.Line1"), textPos - new Vector2(0, 12));
			DrawCenteredExplanationText(Language.GetTextValue($"Mods.{PoTMod.ModName}.Quests.Popups.Line2"), textPos + new Vector2(0, 12));
			List<string> assignedKeys = QuestModPlayer.ToggleQuestUIKey.GetAssignedKeys();
			string key = Language.GetTextValue($"Mods.{PoTMod.ModName}.Quests.Popups.NoKey");

			if (assignedKeys.Count > 0)
			{
				key = assignedKeys[0];
			}

			DrawCenteredExplanationText(Language.GetText($"Mods.{PoTMod.ModName}.Quests.Popups.Line3").Format(key), textPos + new Vector2(0, 36));
		}
	}

	private static string DrawCenteredExplanationText(string text, Vector2 textPos)
	{
		Vector2 size = ChatManager.GetStringSize(FontAssets.ItemStack.Value, text, Vector2.One);
		ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.ItemStack.Value, text, textPos, Color.White * ExplainOpacity, 0f, size / 2f, Vector2.One);
		return text;
	}
}
