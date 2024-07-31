using System.Collections.Generic;
using PathOfTerraria.Content.Items.Gear.Weapons.Grimoire;
using PathOfTerraria.Core.Loaders.UILoading;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.GrimoireSelection;

public class GrimoireInvButton : SmartUIState
{
	public override bool Visible => Main.playerInventory;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Texture2D texture = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/GrimoireButton").Value;
		Vector2 pos = new(GetTextureXPosition(), 220);
		spriteBatch.Draw(texture, pos, null, Color.White, 0, new Vector2(texture.Width / 1.125f, 0), 1, 0, 0);
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		Vector2 pos = new(GetTextureXPosition(), 220);
		var bounding = new Rectangle((int)(pos.X - 64 / 1.125f), (int)pos.Y, 64, 64);

		if (!bounding.Contains(Main.MouseScreen.ToPoint()))
		{
			return;
		}

		SoundEngine.PlaySound(SoundID.MenuOpen);
		int index = 0;

		for (int i = 0; i < Main.LocalPlayer.inventory.Length; ++i)
		{
			Item item = Main.LocalPlayer.inventory[i];

			if (!item.IsAir && item.type == ModContent.ItemType<GrimoireItem>())
			{
				index = i;
				break;
			}
		}

		// Set held item to Grimoire so the UI doesn't instantly close
		Main.LocalPlayer.selectedItem = index;
		UILoader.GetUIState<GrimoireSelectionUIState>().Toggle();
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