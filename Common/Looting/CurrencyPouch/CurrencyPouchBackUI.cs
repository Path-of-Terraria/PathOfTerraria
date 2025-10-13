using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.Looting.CurrencyPouch;

internal class CurrencyPouchBackUI : UIElement
{
	private readonly Asset<Texture2D> texture = null;

	private List<DrawableTooltipLine> tooltips = null;

	public CurrencyPouchBackUI()
	{
		texture = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/CurrencyBack");

		OverrideSamplerState = SamplerState.PointClamp;
		Width.Set(texture.Width(), 0f);
		Height.Set(texture.Height(), 0f);
	}

	public void SetTooltips(List<DrawableTooltipLine> tooltips)
	{
		this.tooltips = tooltips;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		CalculatedStyle dimensions = GetDimensions();
		Texture2D tex = texture.Value;

		Vector2 size = tex.Size();
		Vector2 pos = dimensions.Position() + size;
		pos = pos.Floor();

		spriteBatch.Draw(tex, pos, null, Color.White, 0f, size, Vector2.One, SpriteEffects.None, 0f);

		if (tooltips is not null)
		{
			const int MaxWidth = 388;

			ReLogic.Graphics.DynamicSpriteFont font = FontAssets.ItemStack.Value;
			float count = tooltips.Sum(x => ChatManager.GetStringSize(font, x.Text, x.BaseScale, MaxWidth).Y);

			Vector2 bridgePos = pos - size * new Vector2(1, 0) - new Vector2(0, 8);
			spriteBatch.Draw(tex, bridgePos, new Rectangle(0, 126, 400, 4), Color.White, 0f, Vector2.Zero, new Vector2(1, count / 4f), SpriteEffects.None, 0f);
			spriteBatch.Draw(tex, bridgePos + new Vector2(0, count), new Rectangle(0, 130, 400, 8), Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);

			bridgePos.X += 6;

			foreach (DrawableTooltipLine tooltip in tooltips)
			{
				ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, tooltip.Text, bridgePos, tooltip.Color, 0f, Vector2.Zero, tooltip.BaseScale, MaxWidth);
				bridgePos.Y += ChatManager.GetStringSize(font, tooltip.Text, tooltip.BaseScale, MaxWidth).Y;
			}
		}
	}
}
