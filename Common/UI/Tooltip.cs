﻿using System.Collections.Generic;
using PathOfTerraria.Common.Loaders.UILoading;
using PathOfTerraria.Common.Utilities;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.UI;

/// <summary>
/// Draws the popup tooltip when various elements of the UI are hovered over.
/// </summary>
public class Tooltip : SmartUIState, ILoadable
{
	private static string text = string.Empty;
	private static string tooltip = string.Empty;

	/// <summary>
	/// Width of the drawn tooltip. Defaults and resets to 200 every frame.
	/// </summary>
	public static int DrawWidth { get; set; } = 200;

	public override int DepthPriority => 2;

	public override bool Visible => true;

	public void Load(Mod mod)
	{
		On_Main.Update += Reset;
	}

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text")) + 1;
	}

	/// <summary>
	/// Sets the brightly colored main line of the tooltip. This should be a short descriptor of what you're hovering over, like its name.
	/// </summary>
	/// <param name="name"></param>
	public static void SetName(string name)
	{
		text = name;
	}

	/// <summary>
	/// Sets the more dimly colored 'description' of the tooltip. This should be the 'body' of the tooltip.
	/// </summary>
	/// <param name="newTooltip"></param>
	public static void SetTooltip(string newTooltip)
	{
		ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;
		tooltip = StringUtils.WrapString(newTooltip, DrawWidth * 2, font, 1);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		if (text == string.Empty)
		{
			return;
		}

		ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;

		float nameWidth = ChatManager.GetStringSize(font, text, Vector2.One).X;
		float tipWidth = ChatManager.GetStringSize(font, tooltip, Vector2.One).X * 0.9f;

		float width = Math.Max(nameWidth, tipWidth);
		float height = -16;
		Vector2 pos;

		if (Main.MouseScreen.X > Main.screenWidth - width)
		{
			pos = Main.MouseScreen - new Vector2(width + 20, 0);
		}
		else
		{
			pos = Main.MouseScreen + new Vector2(40, 0);
		}

		height += ChatManager.GetStringSize(font, "{Dummy}\n" + tooltip, Vector2.One).Y + 16;

		if (pos.Y + height > Main.screenHeight)
		{
			pos.Y -= height;
		}

		Utils.DrawInvBG(Main.spriteBatch, new Rectangle((int)pos.X - 10, (int)pos.Y - 10, (int)width + 20, (int)height + 20), new Color(20, 20, 55) * 0.925f);
		Utils.DrawBorderString(Main.spriteBatch, text, pos, Color.White);
		pos.Y += ChatManager.GetStringSize(font, text, Vector2.One).Y + 4;

		Utils.DrawBorderString(Main.spriteBatch, tooltip, pos, Color.LightGray, 0.9f);
	}

	private void Reset(On_Main.orig_Update orig, Main self, GameTime gameTime)
	{
		text = string.Empty;
		tooltip = string.Empty;
		DrawWidth = 200;

		orig(self, gameTime);
	}
}