﻿using PathOfTerraria.Core.Loaders.UILoading;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.PlayerStats;

public class PlayerStatButton : SmartUIState
{
	public override bool Visible => Main.playerInventory;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Texture2D texture = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/PlayerStatButton").Value;
		Vector2 pos = new(GetTextureXPosition(), 150);
		spriteBatch.Draw(texture, pos, null, Color.White, 0, new Vector2(texture.Width / 1.125f, 0), 1, 0, 0);
	}

	public override void SafeClick(UIMouseEvent evt)
	{
		Texture2D texture = ModContent.Request<Texture2D>($"{PathOfTerraria.ModName}/Assets/GUI/PlayerStatButton").Value;
		Vector2 pos = new(GetTextureXPosition(), 150);

		var bounding = new Rectangle((int)(pos.X - texture.Width / 1.125f), (int)pos.Y, texture.Width, texture.Height);

		if (!bounding.Contains(Main.MouseScreen.ToPoint()))
		{
			return;
		}

		SoundEngine.PlaySound(SoundID.MenuOpen);
		UILoader.GetUIState<PlayerStatUIState>().Toggle();
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