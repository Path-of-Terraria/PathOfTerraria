using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using Terraria.UI;
using Terraria.UI.Chat;

namespace PathOfTerraria.Common.UI;

#nullable enable

public struct TooltipDescription()
{
	public required string Identifier;
	public string? SimpleTitle;
	public string? SimpleSubtitle;
	public List<DrawableTooltipLine> Lines = [];
	public Vector2? Position;
	public Item? AssociatedItem;
	public uint VisibilityTimeInTicks = 2;
}

/// <summary>
/// Draws any amount of popup tooltip swhen various elements of the UI are hovered over.
/// </summary>
public class Tooltip : SmartUiState
{
	private struct TooltipPair
	{
		public DrawableTooltipLine Base;
		public DrawableTooltipLine Copy;
	}

	private struct TooltipInstance
	{
		public TooltipDescription Description;
		public uint EndTime;
	}

	private static readonly List<TooltipInstance> tooltips = [];

	public override int DepthPriority => 2;

	public override bool Visible => true;

	public override int InsertionIndex(List<GameInterfaceLayer> layers)
	{
		return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Over"));
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		foreach (ref TooltipInstance tooltip in IterateTooltips())
		{
			Render(tooltip.Description);
		}
	}

	private static Span<TooltipInstance> IterateTooltips()
	{
		uint currentTime = Main.GameUpdateCount;
		for (int i = 0; i < tooltips.Count; i++)
		{
			if (currentTime > tooltips[i].EndTime)
			{
				tooltips.RemoveAt(i--);
			}
		}

		return CollectionsMarshal.AsSpan(tooltips);
	}

	/// <summary> Enqueues a new tooltip to be drawn in the usual interface layer. </summary>
	public static void Create(TooltipDescription args)
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
		tooltip.EndTime = Main.GameUpdateCount + args.VisibilityTimeInTicks;
	}

	/// <summary> Immediately draws the provided tooltip on the screen. </summary>
	private static void Render(TooltipDescription args)
	{
		const int BaseLineSpacing = 0;
		int lineCount = args.Lines.Count + (args.SimpleTitle != null ? 1 : 0) + (args.SimpleSubtitle != null ? 1 : 0);

		// Rent and populate arrays.
		Vector2[] lineMeasures = lineCount != 0 ? ArrayPool<Vector2>.Shared.Rent(lineCount) : [];
		TooltipPair[] lines = lineCount != 0 ? ArrayPool<TooltipPair>.Shared.Rent(lineCount) : [];

		int fillIndex = 0;
		if (args.SimpleTitle != null)
		{
			lines[fillIndex++].Base = new DrawableTooltipLine(new TooltipLine(PoTMod.Instance, "SimpleTitle", args.SimpleTitle), fillIndex, 0, 0, Color.White);
		}
		if (args.SimpleSubtitle != null)
		{
			lines[fillIndex++].Base = new DrawableTooltipLine(new TooltipLine(PoTMod.Instance, "SimpleSubtitle", args.SimpleSubtitle), fillIndex, 0, 0, Color.White);
		}
		foreach (DrawableTooltipLine source in args.Lines)
		{
			lines[fillIndex++].Base = source;
		}

		// Calculate sizes.
		Array.Fill(lineMeasures, default, 0, lineCount);
		Vector2 totalSize = Vector2.Zero;

		for (int i = 0; i < lineCount; i++)
		{
			DrawableTooltipLine line = lines[i].Base;

			// Make a cursed copy of the tooltip line for later.
			lines[i].Copy = new DrawableTooltipLine(line, i, 0, 0, line.Color);

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

			lineMeasures[i] = measure + new Vector2(0f, lineSpacing);
			totalSize.X = Math.Max(totalSize.X, lineMeasures[i].X);
			totalSize.Y += lineMeasures[i].Y + lineSpacing;
		}

		// Calculate adjusted position.
		Vector2 pos = args.Position ?? default;
		if (!args.Position.HasValue)
		{
			// Match Main.MouseTextInner logic.
			const int MouseOffset = 24;
			pos = new Vector2(Main.mouseX + MouseOffset, Main.mouseY + MouseOffset);
		}

		pos.X = Math.Max(0, Math.Min(pos.X, Main.screenWidth - totalSize.X));
		pos.Y = Math.Max(0, Math.Min(pos.Y, Main.screenHeight - totalSize.Y));

		// Draw the background.
		const int BgOffset = 16;
		var bgDstRect = new Rectangle
		{
			X = (int)(pos.X - BgOffset),
			Y = (int)(pos.Y - BgOffset),
			Width = (int)(totalSize.X + BgOffset * 2),
			Height = (int)(totalSize.Y + BgOffset * 2),
		};
		Color bgColor = Color.White * 0.925f;
		Texture2D bgTexture = ModContent.Request<Texture2D>($"{nameof(PathOfTerraria)}/Assets/UI/TooltipBackground", AssetRequestMode.ImmediateLoad).Value;
		RenderBackground(Main.spriteBatch, bgTexture, null, bgDstRect, bgColor);

		// Draw the actual lines.
		Vector2 lineOffset = Vector2.Zero;

		for (int i = 0; i < lineCount; i++)
		{
			DrawableTooltipLine line = lines[i].Base;

			int spacingOffset = 0;
			var drawPoint = (pos + lineOffset).ToPoint();

			// A separate instance is used for Pre/PostDraw here, so as that the two PreDraw calls do not stack up state.
			DrawableTooltipLine lineCopy = lines[i].Copy;
			lineCopy = new DrawableTooltipLine(lineCopy, i, drawPoint.X, drawPoint.Y, lineCopy.Color);

			if (args.AssociatedItem == null || ItemLoader.PreDrawTooltipLine(args.AssociatedItem, lineCopy, ref spacingOffset))
			{
				ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, line.Font, line.Text, drawPoint.ToVector2(), line.OverrideColor ?? line.Color, 0f, line.Origin, line.BaseScale, line.MaxWidth, line.Spread);
				lineOffset.Y += lineMeasures[i].Y + BaseLineSpacing + spacingOffset;
			}

			if (args.AssociatedItem != null)
			{
				ItemLoader.PostDrawTooltipLine(args.AssociatedItem, lineCopy);
			}
		}

		// Return rented arrays.
		if (lineCount != 0)
		{
			ArrayPool<Vector2>.Shared.Return(lineMeasures);
			ArrayPool<TooltipPair>.Shared.Return(lines);
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
		(dw, dh) = (Math.Max(dw, sw), Math.Max(dh, sh));
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