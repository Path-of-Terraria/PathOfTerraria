using System.Collections.Generic;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.PlayerStats;

public class PlayerStatButton : SmartUiState
{
	public override bool Visible => Main.playerInventory;

	private static bool LastHover = false;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Texture2D texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/PlayerStatButton").Value;
		bool hover = UIHelper.GetInvButtonInfo(150, out Vector2 pos);

		if (hover)
		{
			Player player = Main.LocalPlayer;
			player.cursorItemIconText = Language.GetTextValue("Mods.PathOfTerraria.UI.InvButtons.Stats");
			player.noThrow = 2;
			player.cursorItemIconID = -1;
			player.cursorItemIconEnabled = true;
		}

		if (hover != LastHover)
		{
			SoundEngine.PlaySound(LastHover ? SoundID.MenuTick with { Pitch = -0.3f } : SoundID.MenuTick);
		}

		LastHover = hover;

		spriteBatch.Draw(texture, pos, new Rectangle(0, hover ? 64 : 0, 64, 64), Color.White, 0, new Vector2(texture.Width / 1.125f, 0), 1, 0, 0);
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		if (!UIHelper.GetInvButtonInfo(150, out _))
		{
			return;
		}

		SoundEngine.PlaySound(SoundID.MenuOpen);
		SmartUiLoader.GetUiState<PlayerStatUIState>().Toggle();
	}
}