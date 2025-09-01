using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.UI.GrimoireSelection;
using PathOfTerraria.Content.Items.Gear.Weapons.Grimoire;
using PathOfTerraria.Core.UI;
using PathOfTerraria.Core.UI.SmartUI;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.Guide;

public class TutorialRestartButton : SmartUiState
{
	public const int ButtonY = 6;

	public override bool Visible => Main.playerInventory && Main.LocalPlayer.GetModPlayer<TutorialPlayer>().CompletedTutorial;

	private static bool _lastHover = false;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		base.SafeUpdate(gameTime);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Texture2D texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Guide/RestartIcon").Value;
		bool hover = UIHelper.GetInvButtonInfo(ButtonY, out Vector2 pos, new Terraria.DataStructures.Point16(54, 34));

		if (hover)
		{
			Player player = Main.LocalPlayer;
			player.cursorItemIconText = Language.GetTextValue("Mods.PathOfTerraria.UI.InvButtons.RestartTutorial");
			player.noThrow = 2;
			player.cursorItemIconID = -1;
			player.cursorItemIconEnabled = true;
		}

		if (hover != _lastHover)
		{
			SoundEngine.PlaySound(_lastHover ? SoundID.MenuTick with { Pitch = -0.3f } : SoundID.MenuTick);
		}

		_lastHover = hover;

		spriteBatch.Draw(texture, pos, new Rectangle(0, hover ? 36 : 0, 54, 34), Color.White, 0f, new Vector2(texture.Width / 1.125f, 0), 1, 0, 0);
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		if (!UIHelper.GetInvButtonInfo(ButtonY, out _))
		{
			return;
		}

		TutorialUIState.StoredStep = 0;
		TutorialUIState.FromLoad = false;
		Main.LocalPlayer.GetModPlayer<TutorialPlayer>().TutorialStep = 0;
		Main.LocalPlayer.GetModPlayer<TutorialPlayer>().TutorialChecks.Clear();
		Main.LocalPlayer.GetModPlayer<TutorialPlayer>().TutorialChecks.Add(TutorialCheck.RestartedTutorial);
		UIManager.Register("Tutorial UI", "Vanilla: Player Chat", new TutorialUIState(), 0, InterfaceScaleType.UI);
		SoundEngine.PlaySound(SoundID.MenuOpen);
	}
}