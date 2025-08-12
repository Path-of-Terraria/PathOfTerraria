using System.Collections.Generic;
using PathOfTerraria.Common.Systems.Questing;
using PathOfTerraria.Common.UI.Quests;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI;

public class QuestPanelButton : SmartUiState
{
	public override bool Visible => (Main.playerInventory || UIQuestPopupState.FlashQuestButton > 0) && !Main.LocalPlayer.GetModPlayer<QuestModPlayer>().FirstQuest;

	private static Asset<Texture2D> BookTexture = null;
	private static bool LastHover = false;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public override void OnInitialize()
	{
		BookTexture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/QuestBookUi");
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		base.SafeUpdate(gameTime);

		if (UIHelper.GetInvButtonInfo(80, out _))
		{
			Main.LocalPlayer.mouseInterface = true;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Texture2D texture = BookTexture.Value;
		bool hover = UIHelper.GetInvButtonInfo(80, out Vector2 pos);
		Color color = Color.White;

		if (!Main.playerInventory && UIQuestPopupState.FlashQuestButton > 0)
		{
			color *= MathF.Pow(MathF.Sin(Main.GlobalTimeWrappedHourly), 2);

			if (UIQuestPopupState.FlashQuestButton <= 30f)
			{
				color *= UIQuestPopupState.FlashQuestButton / 30f;
			}
		}

		if (hover)
		{
			Player player = Main.LocalPlayer;
			player.cursorItemIconText = Language.GetTextValue("Mods.PathOfTerraria.UI.InvButtons.Quest");
			player.noThrow = 2;
			player.cursorItemIconID = -1;
			player.cursorItemIconEnabled = true;
		}

		if (hover != LastHover)
		{
			SoundEngine.PlaySound(LastHover ? SoundID.MenuTick with { Pitch = -0.3f } : SoundID.MenuTick);
		}

		LastHover = hover;

		spriteBatch.Draw(texture, pos, new Rectangle(0, hover ? 64 : 0, 64, 64), color, 0, new Vector2(texture.Width / 1.125f, 0), 1, 0, 0);
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		Texture2D texture = BookTexture.Value;
		Vector2 pos = new(GetTextureXPosition(), 80);

		//Uses just a 64/64 texture size
		var bounding = new Rectangle((int)(pos.X - texture.Width / 1.125f), (int)pos.Y, 64, 64);

		if (!bounding.Contains(Main.MouseScreen.ToPoint()))
		{
			return;
		}

		SoundEngine.PlaySound(SoundID.MenuOpen);
		SmartUiLoader.GetUiState<QuestsUIState>().Toggle();
		Main.playerInventory = true;
	}

	private static float GetTextureXPosition()
	{
		float screenWidth = Main.screenWidth / 1.12f;
		return screenWidth switch
		{
			//4k or 4k Wide
			>= 2160 => screenWidth / 1.001f,
			//1440p+
			>= 1440 => screenWidth / 1.06f,
			_ => screenWidth / 1.125f
		};
	}
}