using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.Looting.CurrencyPouch;

internal class CurrencyPouchBackUI : UIElement
{
	private List<DrawableTooltipLine> tooltips = null;

	public void SetTooltips(List<DrawableTooltipLine> tooltips)
	{
		this.tooltips = tooltips;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (tooltips is not null)
		{
			const int MaxWidth = 388;

			CalculatedStyle dimensions = GetDimensions();
			ReLogic.Graphics.DynamicSpriteFont font = FontAssets.ItemStack.Value;
			float height = tooltips.Sum(x => ChatManager.GetStringSize(font, x.Text, x.BaseScale, MaxWidth).Y);
			Vector2 tooltipPosition = dimensions.Position() + new Vector2(6f, 6f);
			Rectangle background = new((int)dimensions.X, (int)dimensions.Y, 400, (int)MathF.Ceiling(height) + 12);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, background, new Color(16, 20, 32, 235));

			foreach (DrawableTooltipLine tooltip in tooltips)
			{
				ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, tooltip.Text, tooltipPosition, tooltip.Color, 0f, Vector2.Zero, tooltip.BaseScale, MaxWidth);
				tooltipPosition.Y += ChatManager.GetStringSize(font, tooltip.Text, tooltip.BaseScale, MaxWidth).Y;
			}
		}
	}
}
