using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using PathOfTerraria.Api.Tooltips;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.UI;

#nullable enable

/// <summary> Calculated information needed to render a tooltip. </summary>
internal struct TooltipCache
{
	public int LineCount;
	public Vector2[] LineMeasures;
	public (DrawableTooltipLine Base, DrawableTooltipLine Copy)[] Lines;
	public Vector2 OuterSize;
	public Vector2 Position;
	public Vector2 DesiredPosition;
}

[Obsolete("Use the API namespace.", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
internal struct TooltipDescription()
{
	public required string Identifier;
	public string? SimpleTitle;
	public string? SimpleSubtitle;
	public List<DrawableTooltipLine> Lines = [];
	public Vector2? Position;
	public Vector2 Origin = Vector2.Zero;
	public int Stability;
	public Item? AssociatedItem;
	public uint VisibilityTimeInTicks = 1;
	public Vector2 Padding = new(16, 16);
}

[Obsolete("Use the API namespace.", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class Tooltip
{
	public static void Create(TooltipDescription args)
	{
		Tooltips.Create(new()
		{
			Identifier = args.Identifier,
			SimpleTitle = args.SimpleTitle,
			SimpleSubtitle = args.SimpleSubtitle,
			Lines = args.Lines,
			Position = args.Position,
			Origin = args.Origin,
			Stability = args.Stability,
			AssociatedItem = args.AssociatedItem,
			VisibilityTimeInTicks = args.VisibilityTimeInTicks,
			Padding = args.Padding,
		});
	}
}

internal class TooltipsImpl : SmartUiState
{
	private struct TooltipInstance
	{
		public Api.Tooltips.TooltipDescription Description;
		public TooltipCache Cache;
		public uint EndTime;
	}

	private static readonly List<TooltipInstance> tooltips = [];

	// Main.GameUpdateCount doesn't increment normally when autopause is on, this is the workaround
	private static uint tickCount;
	private static uint lastTickCount;

	public override int DepthPriority => 2;

	public override bool Visible => true;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return Math.Max(0, layers.FindIndex(layer => layer.Name.Equals("Vanilla: Cursor")) - 1);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		tickCount++;

		Span<TooltipInstance> span = IterateTooltips();

		foreach (ref TooltipInstance tooltip in span)
		{
			Recalculate(in tooltip.Description, ref tooltip.Cache);
		}

		ResolveTooltipCollisions(span);

		foreach (ref TooltipInstance tooltip in span)
		{
			Render(in tooltip.Description, tooltip.Cache);
		}
	}

	private static Span<TooltipInstance> IterateTooltips()
	{
		// Remove expired tooltips. Account for tick count resets, which are likely to occur if moving between subworlds.
		uint currentTime = tickCount;
		uint previousTime = lastTickCount;
		lastTickCount = currentTime;

		if (currentTime < previousTime)
		{
			tooltips.Clear();
		}
		else
		{
			for (int i = 0; i < tooltips.Count; i++)
			{
				if (currentTime > tooltips[i].EndTime)
				{
					tooltips.RemoveAt(i--);
				}
			}
		}

		return CollectionsMarshal.AsSpan(tooltips);
	}

	public static void Create(Api.Tooltips.TooltipDescription args)
	{
		if (string.IsNullOrEmpty(args.Identifier))
		{
			throw new ArgumentException("A proper tooltip identifier must be provided.");
		}

		int tooltipIndex;
		if (tooltips.FindIndex(t => t.Description.Identifier == args.Identifier) is >= 0 and int existing)
		{
			tooltipIndex = existing;
		}
		else
		{
			tooltipIndex = tooltips.Count;
			tooltips.Add(new TooltipInstance());
		}

		ref TooltipInstance tooltip = ref CollectionsMarshal.AsSpan(tooltips)[tooltipIndex];
		tooltip.Description = args;
		tooltip.EndTime = tickCount + args.VisibilityTimeInTicks;
	}

	private static void Recalculate(in Api.Tooltips.TooltipDescription args, ref TooltipCache cache)
	{
		const int BaseLineSpacing = 0;
		int lineCount = args.Lines.Count + (args.SimpleTitle != null ? 1 : 0) + (args.SimpleSubtitle != null ? 1 : 0);

		// Populate arrays.
		cache.LineCount = lineCount;
		Array.Resize(ref cache.Lines, Math.Max(cache.Lines?.Length ?? 0, lineCount));
		Array.Resize(ref cache.LineMeasures, Math.Max(cache.LineMeasures?.Length ?? 0, lineCount));

		int fillIndex = 0;
		if (args.SimpleTitle != null)
		{
			cache.Lines[fillIndex++].Base = new DrawableTooltipLine(new TooltipLine(PoTMod.Instance, "SimpleTitle", args.SimpleTitle), fillIndex, 0, 0, Color.White);
		}
		if (args.SimpleSubtitle != null)
		{
			cache.Lines[fillIndex++].Base = new DrawableTooltipLine(new TooltipLine(PoTMod.Instance, "SimpleSubtitle", args.SimpleSubtitle), fillIndex, 0, 0, Color.White);
		}
		foreach (DrawableTooltipLine source in args.Lines)
		{
			cache.Lines[fillIndex++].Base = source;
		}

		// Calculate sizes.
		cache.OuterSize = default;
		Array.Fill(cache.LineMeasures, default, 0, lineCount);

		for (int i = 0; i < lineCount; i++)
		{
			DrawableTooltipLine line = cache.Lines[i].Base;

			// Make a cursed copy of the tooltip line for later.
			cache.Lines[i].Copy = new DrawableTooltipLine(line, i, 0, 0, line.Color);

			// Point of caution:
			// To acquire information necessary for correct bounding box calculations, there is no choice but to call PreDrawTooltipLine twice.
			// This is a TML design issue -- Mirsario.
			int spacingOffset = 0;
			if (args.AssociatedItem != null && !ItemLoader.PreDrawTooltipLine(args.AssociatedItem, line, ref spacingOffset))
			{
				lineCount--;
				continue;
			}

			Vector2 measure = ChatManager.GetStringSize(line.Font, line.Text, line.BaseScale, line.MaxWidth);
			int newLineCount = line.Text.Count(c => c == '\n');
			float lineSpacing = BaseLineSpacing + spacingOffset;

			cache.LineMeasures[i] = measure;
			cache.OuterSize.X = Math.Max(cache.OuterSize.X, cache.LineMeasures[i].X);
			cache.OuterSize.Y += cache.LineMeasures[i].Y + lineSpacing;
		}
		// Add padding.
		cache.OuterSize += args.Padding * 2f;

		// Calculate the top-left position.
		cache.Position = args.Position ?? default;
		if (!args.Position.HasValue)
		{
			// Match Main.MouseTextInner logic.
			const int MouseOffset = 8;
			cache.Position = new Vector2(Main.mouseX + MouseOffset, Main.mouseY + MouseOffset);
		}
		// Adjust by origin.
		cache.Position.X = MathHelper.Lerp(cache.Position.X, cache.Position.X - cache.OuterSize.X, args.Origin.X);
		cache.Position.Y = MathHelper.Lerp(cache.Position.Y, cache.Position.Y - cache.OuterSize.Y, args.Origin.Y);
		// Make a copy of the computed position in case it is adjusted further.
		cache.DesiredPosition = cache.Position;
		// Adjust to screen bounds.
		cache.Position.X = Math.Max(0, Math.Min(cache.Position.X, Main.screenWidth - cache.OuterSize.X));
		cache.Position.Y = Math.Max(0, Math.Min(cache.Position.Y, Main.screenHeight - cache.OuterSize.Y));
	}

	private static void ResolveTooltipCollisions(Span<TooltipInstance> tooltips)
	{
		for (int i = 0; i < tooltips.Length; i++)
		{
			for (int j = 0; j < tooltips.Length; j++)
			{
				if (i == j) { continue; }

				ref TooltipCache a = ref tooltips[i].Cache; // Self
				ref readonly TooltipCache b = ref tooltips[j].Cache; // Other

				if (tooltips[i].Description.Stability > tooltips[j].Description.Stability) { continue; }

				var aBounds = new Vector4(a.Position, a.Position.X + a.OuterSize.X, a.Position.Y + a.OuterSize.Y);
				var bBounds = new Vector4(b.Position, b.Position.X + b.OuterSize.X, b.Position.Y + b.OuterSize.Y);

				if (aBounds.X <= bBounds.Z && aBounds.Y <= bBounds.W && aBounds.Z >= bBounds.X && aBounds.W >= bBounds.Y)
				{
					Vector2 aCenter = a.DesiredPosition + a.OuterSize * 0.5f;
					Vector2 bCenter = b.DesiredPosition + b.OuterSize * 0.5f;
					a.Position.X = aCenter.X <= bCenter.X ? bBounds.X - a.OuterSize.X : bBounds.Z;

					// Adjust to screen bounds.
					a.Position.X = Math.Max(0, Math.Min(a.Position.X, Main.screenWidth - a.OuterSize.X));
					a.Position.Y = Math.Max(0, Math.Min(a.Position.Y, Main.screenHeight - a.OuterSize.Y));
				}
			}
		}
	}

	/// <summary> Immediately draws the provided tooltip on the screen, using the provided cache. </summary>
	public static void Render(in Api.Tooltips.TooltipDescription args, in TooltipCache cache)
	{
		// Draw the background.
		var bgDstRect = new Rectangle
		{
			X = (int)cache.Position.X,
			Y = (int)cache.Position.Y,
			Width = (int)cache.OuterSize.X,
			Height = (int)cache.OuterSize.Y,
		};
		Color bgColor = Color.White * 0.925f;
		Texture2D bgTexture = ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/UI/TooltipBackground", AssetRequestMode.ImmediateLoad).Value;
		RenderBackground(Main.spriteBatch, bgTexture, null, bgDstRect, bgColor);

		// Draw the actual lines.
		Vector2 lineOffset = args.Padding;

		for (int i = 0; i < cache.LineCount; i++)
		{
			DrawableTooltipLine line = cache.Lines[i].Base;

			int spacingOffset = 0;
			var drawPoint = (cache.Position + lineOffset).ToPoint();

			// A separate instance is used for Pre/PostDraw here, so as that the two PreDraw calls do not stack up state.
			DrawableTooltipLine lineCopy = cache.Lines[i].Copy;
			lineCopy = new DrawableTooltipLine(lineCopy, i, drawPoint.X, drawPoint.Y, lineCopy.Color);

			if (args.AssociatedItem == null || ItemLoader.PreDrawTooltipLine(args.AssociatedItem, lineCopy, ref spacingOffset))
			{
				ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, line.Font, line.Text, drawPoint.ToVector2(), line.OverrideColor ?? line.Color, 0f, line.Origin, line.BaseScale, line.MaxWidth, line.Spread);
				lineOffset.Y += cache.LineMeasures[i].Y + spacingOffset;
			}

			if (args.AssociatedItem != null)
			{
				ItemLoader.PostDrawTooltipLine(args.AssociatedItem, lineCopy);
			}
		}
	}

	private static void RenderBackground(SpriteBatch sb, Texture2D texture, Rectangle? sourceRectangle, Rectangle destinationRectangle, Color color)
	{
		Rectangle src = sourceRectangle ?? texture.Bounds;
		Rectangle dst = destinationRectangle;
		(int sw, int sh) = (src.Width, src.Height); // Source Size
		(int dw, int dh) = (dst.Width, dst.Height); // Destination Size
		(int fw, int fh) = (texture.Width / 3, texture.Height / 3); // Frame size
		(int fW, int fH) = (fw + fw, fh + fh); // Double frame size
		(int xx, int yy) = (dst.X, dst.Y); // Position

		// Impose a minimum size.
		(dw, dh) = (Math.Max(dw, fW), Math.Max(dh, fH));
		// Align size to the pixel grid.
		const int AlignX = 4;
		const int AlignY = 4;
		dw = (int)MathF.Ceiling(dw / (float)AlignX) * AlignX;
		dh = (int)MathF.Ceiling(dh / (float)AlignY) * AlignY;

		// Render the tiles.
		int xTiles = (int)MathF.Floor(dw / (float)fw);
		int yTiles = (int)MathF.Floor(dh / (float)fh);
		float xTilesStep = dw / (float)fw - xTiles;
		float yTilesStep = dh / (float)fh - yTiles;

		for (int y = 0, yOffset = 0; y <= yTiles; y++)
		{
			for (int x = 0, xOffset = 0; x <= xTiles; x++)
			{
				int xFrame = x == 0 ? 0 : (x == xTiles ? 2 : 1);
				int yFrame = y == 0 ? 0 : (y == yTiles ? 2 : 1);
				int xWidth = (x == xTiles - 1) ? (int)(fw * xTilesStep) : fw;
				int yWidth = (y == yTiles - 1) ? (int)(fh * yTilesStep) : fh;

				sb.Draw(
					texture,
					new Rectangle(xx + xOffset, yy + yOffset, xWidth, yWidth),
					new Rectangle(xFrame * fw, yFrame * fh, xWidth, yWidth),
					color
				);

				xOffset += xWidth;
				yOffset += x == xTiles ? yWidth : 0;
			}
		}
	}
}