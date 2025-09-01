using System.Collections.Generic;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Content.Items.Gear.Weapons.Grimoire;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.GrimoireSelection;

public class GrimoireInvButton : SmartUiState
{
	public override bool Visible => Main.playerInventory && GrimoirePlayer.Get().HasObtainedGrimoire;

	private static int _denyTimer = 0;
	private static bool _lastHover = false;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		base.SafeUpdate(gameTime);

		_denyTimer--;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Texture2D texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Grimoire/GrimoireButton").Value;
		var color = Color.Lerp(Color.White, Color.Red, _denyTimer / 20f);
		bool hover = UIHelper.GetInvButtonInfo(220, out Vector2 pos);

		if (hover)
		{
			Player player = Main.LocalPlayer;
			player.cursorItemIconText = Language.GetTextValue("Mods.PathOfTerraria.UI.InvButtons." + (GetGrimoireIndex() == -1 ? "NoGrimoire" : "Grimoire"));
			player.noThrow = 2;
			player.cursorItemIconID = -1;
			player.cursorItemIconEnabled = true;
		}

		if (hover != _lastHover)
		{
			SoundEngine.PlaySound(_lastHover ? SoundID.MenuTick with { Pitch = -0.3f } : SoundID.MenuTick);
		}

		_lastHover = hover;

		spriteBatch.Draw(texture, pos, new Rectangle(0, hover ? 64 : 0, 64, 64), color, MathF.Max(0, _denyTimer * 0.005f), new Vector2(texture.Width / 1.125f, 0), 1, 0, 0);
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		if (!UIHelper.GetInvButtonInfo(220, out _))
		{
			return;
		}

		int index = GetGrimoireIndex();

		if (index != -1)
		{
			// Set held item to Grimoire so the UI doesn't instantly close
			Main.LocalPlayer.selectedItem = index;
			SmartUiLoader.GetUiState<GrimoireSelectionUIState>().Toggle();
			SoundEngine.PlaySound(SoundID.MenuOpen);
		}
		else
		{
			_denyTimer = 20;
		}
	}

	private static int GetGrimoireIndex()
	{
		int index = -1;

		for (int i = 0; i < Main.LocalPlayer.inventory.Length; ++i)
		{
			Item item = Main.LocalPlayer.inventory[i];

			if (!item.IsAir && item.type == ModContent.ItemType<GrimoireItem>())
			{
				index = i;
				break;
			}
		}

		return index;
	}
}