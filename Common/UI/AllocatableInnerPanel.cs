using System.Collections.Generic;
using System.Runtime.InteropServices;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Utilities;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.Utilities;

namespace PathOfTerraria.Common.UI;

internal abstract class AllocatableInnerPanel : SmartUiElement
{
	private static readonly Asset<Texture2D> _chainTex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Link");

	private readonly HashSet<UIElement> _draggable = [];

	private readonly Dictionary<UIElement, (float OrigLeft, float OrigTop, float OrigWidth, float OrigHeight)> _origPositions = [];

	private const float ZoomMin = 0.2f;
	private const float ZoomMax = 2.25f;
	private const float ZoomStep = 0.15f;

	/// <summary> Whether this panel supports scroll-wheel zoom. Override in a subclass to enable. </summary>
	protected virtual bool EnableZoom => false;

	/// <summary> Target zoom level that the current zoom will interpolate towards. </summary>
	public float TargetZoom { get; private set; } = 1f;

	/// <summary> Current zoom level. 1 is the default; range is [<see cref="ZoomMin"/>, <see cref="ZoomMax"/>]. </summary>
	public float Zoom { get; private set; } = 1f;
	
	/// <summary> Target drag center that interpolates along with zoom. </summary>
	private Vector2 _targetDragCenter;
	
	protected Vector2 DragOffset;
	protected Vector2 DragCenter;

	public List<Edge<IConnectedAllocatableNode>> Connections { get; } = [];

	public AllocatableInnerPanel()
	{
		Width = Height = StyleDimension.Fill;
		OverflowHidden = true;
	}

	/// <summary> Appends <paramref name="e"/> and allows it to be dragged. </summary>
	public void AppendAsDraggable(UIElement e)
	{
		Append(e);
		_draggable.Add(e);
		_origPositions[e] = (e.Left.Pixels, e.Top.Pixels, e.Width.Pixels, e.Height.Pixels);
	}

	protected override void DrawChildren(SpriteBatch spriteBatch)
	{
		// Drawing connections here means it gets clipped by OverflowHidden correctly
		DrawEdgeConnections(spriteBatch, CollectionsMarshal.AsSpan(Connections), Zoom);
		
		base.DrawChildren(spriteBatch);
	}

	public static void DrawEdgeConnections(SpriteBatch spriteBatch, ReadOnlySpan<Edge<IConnectedAllocatableNode>> connections, float scale = 1f)
	{
		Vector2 center = default;
		Texture2D chainTex = _chainTex.Value;

		foreach (Edge<IConnectedAllocatableNode> edge in connections)
		{
			Color color = Color.Gray;
			IConnectedAllocatableNode start = edge.Start;
			IConnectedAllocatableNode end = edge.End;

			if (start == null || end == null)
			{
				continue;
			}

			if (edge.Flags.HasFlag(EdgeFlags.Hidden))
			{
				continue;
			}

			if (end.AppearsAsCanBeAllocated() && end.AppearsAsAllocated())
			{
				color = Color.Lerp(Color.Gray, Color.White,
					(float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);
			}

			if (end.AppearsAsAllocated() && start.AppearsAsAllocated())
			{
				color = Color.White;
			}

			Vector2 startPos = start.GetCenter();
			Vector2 endPos = end.GetCenter();

			if (!edge.Flags.HasFlag(EdgeFlags.EffectsOnly))
			{
				// Step size keeps link count constant regardless of zoom; each link is drawn at the current scale.
				float linkSize = 16 * scale;
				for (float k = 0; k <= 1; k += 1 / (Vector2.Distance(startPos, endPos) / linkSize))
				{
					Vector2 pos = center + Vector2.Lerp(startPos, endPos, k);
					spriteBatch.Draw(chainTex, pos, null, color, startPos.DirectionTo(endPos).ToRotation(), chainTex.Size() / 2, scale, 0, 0);
				}
			}

			bool showParticles = start.AppearsAsAllocated() && (end.AppearsAsAllocated() || (edge.Flags.HasFlag(EdgeFlags.EffectsOnly) && end.AppearsAsCanBeAllocated()));

			if (showParticles)
			{
				Texture2D glow = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/GlowAlpha").Value;
				var glowColor = new Color(255, 230, 150) { A = 0 };

				var rand = new FastRandom(edge.GetHashCode());

				for (int k = 0; k < 8; k++)
				{
					float dist = Vector2.Distance(startPos, endPos);
					float len = (40 + rand.Next(120)) * dist / 50;
					float particleScale = 0.05f + rand.Next(10000) / 10000f * 0.15f;
					
					float progress = (Main.GameUpdateCount + 15 * k) % len / len;
					Vector2 pos = center + Vector2.SmoothStep(startPos, endPos, progress);
					float scale2 = (float)Math.Sin(progress * 3.14f) * (0.4f - particleScale) * scale;
					
					spriteBatch.Draw(glow, pos, null, glowColor * scale2, 0, glow.Size() / 2f, scale2, 0, 0);
				}
			}
		}
	}

	/// <summary><inheritdoc cref="SmartUiElement.SafeUpdate(GameTime)"/><br/>
	/// Also handles repositioning by dragging the cursor </summary>
	public override void SafeUpdate(GameTime gameTime)
	{
		Vector2 oldOffset = DragOffset;
		DragOffset = Main.MouseScreen;

		// Smoothly interpolate zoom and drag center towards targets
		float oldZoom = Zoom;
		Zoom = MathUtils.Damp(Zoom, TargetZoom, 0.02f, (float)gameTime.ElapsedGameTime.TotalSeconds);
		DragCenter = MathUtils.Damp(DragCenter, _targetDragCenter, 0.02f, (float)gameTime.ElapsedGameTime.TotalSeconds);

		// If zoom changed due to interpolation, update element positions
		if (Math.Abs(Zoom - oldZoom) > 0.001f)
		{
			foreach (UIElement c in _draggable)
			{
				if (!_origPositions.TryGetValue(c, out var orig))
				{
					continue;
				}

				c.Left.Pixels = orig.OrigLeft * Zoom + DragCenter.X;
				c.Top.Pixels = orig.OrigTop * Zoom + DragCenter.Y;
				c.Width.Pixels = orig.OrigWidth * Zoom;
				c.Height.Pixels = orig.OrigHeight * Zoom;
			}

			Recalculate();
		}

		if (ContainsPoint(Main.MouseScreen) && Main.mouseLeft) //Manually check mouse input because other elements shouldn't be allowed to interfere
		{
			Vector2 velocity = DragOffset - oldOffset;
			DragCenter += velocity;
			_targetDragCenter += velocity; // Keep target in sync when dragging

			foreach (UIElement c in _draggable)
			{
				c.Left.Pixels += velocity.X;
				c.Top.Pixels += velocity.Y;
			}

			Recalculate();
		}
	}
	
	public override void SafeScrollWheel(UIScrollWheelEvent evt)
	{
		if (!EnableZoom || !ContainsPoint(Main.MouseScreen))
		{
			return;
		}

		float delta = evt.ScrollWheelValue > 0 ? ZoomStep : -ZoomStep;
		float newTargetZoom = Math.Clamp(TargetZoom + delta, ZoomMin, ZoomMax);

		if (newTargetZoom == TargetZoom)
		{
			return;
		}

		// Zoom toward the mouse cursor so the point under the cursor stays fixed.
		CalculatedStyle dims = GetDimensions();
		var panelCenter = new Vector2(dims.X + dims.Width * 0.5f, dims.Y + dims.Height * 0.5f);
		Vector2 mouseRelCenter = Main.MouseScreen - panelCenter;

		// Calculate the new target drag center based on zoom-to-mouse logic
		float ratio = newTargetZoom / TargetZoom;
		_targetDragCenter = mouseRelCenter * (1f - ratio) + _targetDragCenter * ratio;

		TargetZoom = newTargetZoom;
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		base.DrawSelf(spriteBatch);
		
		// Draw zoom bar if zoom is enabled
		if (EnableZoom)
		{
			DrawZoomBar(spriteBatch);
		}
	}

	private void DrawZoomBar(SpriteBatch spriteBatch)
	{
		CalculatedStyle dims = GetDimensions();
		
		// Position in middle-right
		const float barWidth = 12f;
		const float barHeight = 120f;
		const float margin = 20f;
		
		Vector2 barPos = new Vector2(dims.X + dims.Width - barWidth - margin, dims.Y + (dims.Height - barHeight) * 0.5f);
		
		// Background
		var bgRect = new Rectangle((int)barPos.X, (int)barPos.Y, (int)barWidth, (int)barHeight);
		spriteBatch.Draw(TextureAssets.MagicPixel.Value, bgRect, Color.Black * 0.6f);
		
		// Fill based on zoom level
		float zoomProgress = (Zoom - ZoomMin) / (ZoomMax - ZoomMin);
		float fillHeight = (barHeight - 2) * zoomProgress;
		var fillRect = new Rectangle((int)barPos.X + 1, (int)(barPos.Y + barHeight - 1 - fillHeight), (int)barWidth - 2, (int)fillHeight);
		Color fillColor = Color.Lerp(Color.Orange, Color.Cyan, zoomProgress);
		spriteBatch.Draw(TextureAssets.MagicPixel.Value, fillRect, fillColor);
		
		var topBorder = new Rectangle((int)barPos.X, (int)barPos.Y, (int)barWidth, 1);
		var bottomBorder = new Rectangle((int)barPos.X, (int)(barPos.Y + barHeight - 1), (int)barWidth, 1);
		var leftBorder = new Rectangle((int)barPos.X, (int)barPos.Y, 1, (int)barHeight);
		var rightBorder = new Rectangle((int)(barPos.X + barWidth - 1), (int)barPos.Y, 1, (int)barHeight);
		
		Color borderColor = Color.White * 0.8f;
		spriteBatch.Draw(TextureAssets.MagicPixel.Value, topBorder, borderColor);
		spriteBatch.Draw(TextureAssets.MagicPixel.Value, bottomBorder, borderColor);
		spriteBatch.Draw(TextureAssets.MagicPixel.Value, leftBorder, borderColor);
		spriteBatch.Draw(TextureAssets.MagicPixel.Value, rightBorder, borderColor);
		
		// Zoom percentage text (to the left of the bar)
		string zoomText = $"{(int)(Zoom * 100)}%";
		Vector2 textSize = FontAssets.MouseText.Value.MeasureString(zoomText) * 0.8f;
		Vector2 textPos = barPos + new Vector2(-textSize.X - 8f, (barHeight - textSize.Y) * 0.5f);
		Utils.DrawBorderString(spriteBatch, zoomText, textPos, Color.White, 0.8f);
	}
}