using PathOfTerraria.Common.UI;
using PathOfTerraria.Core.UI;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.Looting.CurrencyPouch;

public class CurrencyPouchIcon : SmartUiState
{
	public override bool Visible => Main.playerInventory;

	private static int NewXPosition => (int)UIHelper.GetTextureXPosition() - 64;

	private static Asset<Texture2D> Texture = null;
	private static bool LastHover = false;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public override void OnInitialize()
	{
		Texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CurrencyPouchButton");
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		base.SafeUpdate(gameTime);

		if (UIHelper.GetInvButtonInfo(150, out _, null, NewXPosition))
		{
			Main.LocalPlayer.mouseInterface = true;
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Texture2D texture = Texture.Value;
		bool hover = UIHelper.GetInvButtonInfo(150, out Vector2 pos, null, NewXPosition);
		Color color = Color.White;

		if (hover)
		{
			Player player = Main.LocalPlayer;
			player.cursorItemIconText = Language.GetTextValue("Mods.PathOfTerraria.UI.InvButtons.CurrencyPouch");
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
		Texture2D texture = Texture.Value;
		Vector2 pos = new(UIHelper.GetTextureXPosition(), 150);

		//Uses just a 64/64 texture size
		var bounding = new Rectangle((int)(pos.X - texture.Width / 1.125f) - 64, (int)pos.Y, 64, 64);

		if (!bounding.Contains(Main.MouseScreen.ToPoint()))
		{
			return;
		}

		SoundEngine.PlaySound(SoundID.MenuOpen);
		UIManager.TryToggleOrRegister(CurrencyPouchUIState.Identifier, "Vanilla: Mouse Text", new CurrencyPouchUIState(), 0, InterfaceScaleType.UI);
	}
}