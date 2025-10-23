using Microsoft.Build.Tasks.Hosting;
using ReLogic.Content;
using ReLogic.Graphics;
using System.Globalization;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.NPCs.Dialogue;

public class TextureTagHandler : ITagHandler
{
	public TextSnippet Parse(string text, Color baseColor, string options)
	{
		float parsedScale = 1f;
		float yOffset = 0f;
		bool hard = false;

		if (!string.IsNullOrEmpty(options))
		{
			foreach (string opt in options.Split(','))
			{
				if (opt.StartsWith('s') && float.TryParse(opt.AsSpan(1), NumberStyles.Float, CultureInfo.InvariantCulture, out float s))
				{
					parsedScale = s;
				}

				if (opt.StartsWith("y=") && float.TryParse(opt.AsSpan(2), NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
				{
					yOffset = y;
				}

				if (opt.Equals("h", StringComparison.OrdinalIgnoreCase))
				{
					hard = true;
				}
			}
		}

		Asset<Texture2D> asset = ModContent.Request<Texture2D>(text.Trim(), AssetRequestMode.ImmediateLoad);
		return new TextureSnippet(asset, parsedScale, yOffset, hard) { Color = baseColor };
	}

	private sealed class TextureSnippet : TextSnippet
	{
		private readonly Asset<Texture2D> _tex;
		private readonly float _scale;
		private readonly float _yOffset;
		private readonly bool _hard;

		public TextureSnippet(Asset<Texture2D> tex, float scale, float yOffset, bool hard)
		{
			_tex = tex;
			_scale = scale;
			_yOffset = yOffset;
			_hard = hard;

			Text = "";
			CheckForHover = true;
		}

		public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1f)
		{
			Texture2D tex = _tex.Value;
			float drawScale = _scale * (_hard ? 1 : scale);
			float padX = 2f;

			// Height at least one chat line so it sits nicely on the baseline
			float lineHeight = FontAssets.MouseText.Value.LineSpacing * scale;
			float texW = tex.Width * drawScale;
			float texH = tex.Height * drawScale;
			float allocW = texW + padX;
			float allocH = Math.Max(texH, lineHeight);

			size = new Vector2(allocW, allocH);

			if (justCheckingString)
			{
				return true;
			}

			if (color.R == 0 && color.G == 0 && color.B == 0)
			{
				return true;
			}

			Vector2 drawPos = position + new Vector2(0f, (allocH - texH) * 0.5f + _yOffset);
			drawPos.Floor();

			spriteBatch.Draw(tex, drawPos, null, Color.White, 0f, Vector2.Zero, drawScale, SpriteEffects.None, 0f);
			return true;
		}

		// Not used when UniqueDraw returns true, but good to keep consistent with width
		public override float GetStringLength(DynamicSpriteFont _)
		{
			return _tex.Value.Width * _scale + 2f;
		}

		public override void OnHover()
		{
			if (_tex.Name.Contains("Developer"))
			{
				Main.instance.MouseText("A developer of Path of Terraria");
				return;
			}

			if (_tex.Name.Contains("Moderator"))
			{
				Main.instance.MouseText("A moderator of Path of Terraria.");
				return;
			}

			Main.instance.MouseText("A supporter of Path of Terraria.");
		}
	}
}