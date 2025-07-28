using PathOfTerraria.Common.UI;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.Systems.ModPlayers.LivesSystem;

internal class BossDomainLivesUILayer : ILoadable
{
	public void Load(Mod mod)
	{
		MiscOverlayUI.DrawOverlay += DrawLivesCounter;
	}

	public void Unload()
	{
		MiscOverlayUI.DrawOverlay += DrawLivesCounter;
	}

	private void DrawLivesCounter(SpriteBatch spriteBatch)
	{
		if (!Main.LocalPlayer.GetModPlayer<BossDomainLivesPlayer>().InDomain)
		{
			return;
		}

		int lives = Main.LocalPlayer.GetModPlayer<BossDomainLivesPlayer>().LivesLeft;
		string text = lives + " " + Language.GetTextValue("Mods.PathOfTerraria.UI.Lives");
		ReLogic.Graphics.DynamicSpriteFont font = FontAssets.MouseText.Value;
		var position = new Vector2(Main.screenWidth - ChatManager.GetStringSize(font, text, Vector2.One).X - 8, 0);
		ChatManager.DrawColorCodedString(spriteBatch, font, text, position, Main.MouseTextColorReal, 0f, Vector2.Zero, Vector2.One);
	}
}
