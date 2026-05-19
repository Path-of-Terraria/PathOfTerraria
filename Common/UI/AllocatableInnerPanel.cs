using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Input;
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
	private static readonly Asset<Texture2D> _glowTex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/GlowAlpha");
	private static readonly Asset<Effect> _connectorShader = ModContent.Request<Effect>($"{PoTMod.ModName}/Assets/Effects/PassiveTreeConnector");

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
		Texture2D glowTex = _glowTex.Value;
		Effect connectorEffect = _connectorShader.Value;
		SpriteBatchArgs args = spriteBatch.GetArguments();
		float time = (float)Main.timeForVisualEffects;

		foreach (Edge<IConnectedAllocatableNode> edge in connections)
		{
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

			bool startAllocated = start.AppearsAsAllocated();
			bool endAllocated = end.AppearsAsAllocated();
			bool endCanAllocate = end.AppearsAsCanBeAllocated();
			bool fullyAllocated = startAllocated && endAllocated;
			bool highlighted = endCanAllocate && endAllocated;
			float pulse = highlighted ? ((float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f) : 0f;
			Vector2 startPos = start.GetCenter();
			Vector2 endPos = end.GetCenter();

			if (!edge.Flags.HasFlag(EdgeFlags.EffectsOnly))
			{
				GetConstellationPalette(fullyAllocated, highlighted, pulse, out Color coreColor, out Color glowColor, out Color nodeColor);
				connectorEffect.Parameters["uTime"].SetValue(time * 0.85f);
				connectorEffect.Parameters["uPulse"].SetValue(pulse);
				connectorEffect.Parameters["uShimmer"].SetValue(fullyAllocated ? 0.35f : (highlighted ? 0.28f : 0.18f));
				connectorEffect.Parameters["uTint"].SetValue(coreColor.ToVector3());

				using (spriteBatch.Override(args with { Effect = connectorEffect }))
				{
					DrawLine(spriteBatch, startPos, endPos, glowColor * 0.65f, 4.6f * scale);
					DrawLine(spriteBatch, startPos, endPos, coreColor, 2.1f * scale);
					DrawLine(spriteBatch, startPos, endPos, Color.White * (0.3f + pulse * 0.2f), 0.85f * scale);
				}

				DrawConstellationNodes(spriteBatch, glowTex, startPos, endPos, nodeColor, scale, edge.GetHashCode(), pulse);
			}

			bool showParticles = startAllocated && (endAllocated || (edge.Flags.HasFlag(EdgeFlags.EffectsOnly) && endCanAllocate));

			if (showParticles)
			{
				var travelingGlowColor = new Color(130, 210, 255) { A = 0 };

				var rand = new FastRandom(edge.GetHashCode());

				for (int k = 0; k < 8; k++)
				{
					float dist = Vector2.Distance(startPos, endPos);
					float len = (40 + rand.Next(120)) * dist / 50;
					float particleScale = 0.05f + rand.Next(10000) / 10000f * 0.15f;
					
					float progress = (Main.GameUpdateCount + 15 * k) % len / len;
					Vector2 pos = center + Vector2.SmoothStep(startPos, endPos, progress);
					float scale2 = (float)Math.Sin(progress * 3.14f) * (0.4f - particleScale) * scale;
					
					spriteBatch.Draw(glowTex, pos, null, travelingGlowColor * scale2, 0, glowTex.Size() / 2f, scale2, 0, 0);
				}
			}
		}
	}

	private static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
	{
		Vector2 delta = end - start;
		float length = delta.Length();

		if (length <= 0.001f)
		{
			return;
		}

		float rotation = delta.ToRotation();
		Vector2 scale = new(length, thickness);
		spriteBatch.Draw(TextureAssets.MagicPixel.Value, start, null, color, rotation, new Vector2(0f, 0.5f), scale, SpriteEffects.None, 0f);
	}

	private static void DrawConstellationNodes(SpriteBatch spriteBatch, Texture2D glowTex, Vector2 start, Vector2 end, Color color, float zoomScale, int seed, float pulse)
	{
		float dist = Vector2.Distance(start, end);
		float time = (float)Main.timeForVisualEffects;
		float nodePulse = 0.88f + 0.12f * (float)Math.Sin(time * 2.7f + seed * 0.005f);
		DrawStarNode(spriteBatch, glowTex, start, color, (0.21f + pulse * 0.03f) * zoomScale * nodePulse);
		DrawStarNode(spriteBatch, glowTex, end, color, (0.21f + pulse * 0.03f) * zoomScale * nodePulse);

		int interiorNodeCount = Math.Clamp((int)(dist / (72f * Math.Max(zoomScale, 0.4f))), 1, 3);

		for (int i = 1; i <= interiorNodeCount; i++)
		{
			float progress = i / (interiorNodeCount + 1f);
			Vector2 pos = Vector2.Lerp(start, end, progress);
			float twinkle = 0.75f + 0.25f * (float)Math.Sin(time * 3.6f + i * 0.8f + seed * 0.004f);
			DrawStarNode(spriteBatch, glowTex, pos, color * 0.75f, (0.1f + pulse * 0.02f) * zoomScale * twinkle);
		}
	}

	private static void DrawStarNode(SpriteBatch spriteBatch, Texture2D glowTex, Vector2 position, Color color, float scale)
	{
		if (scale <= 0.001f)
		{
			return;
		}

		spriteBatch.Draw(glowTex, position, null, color, 0f, glowTex.Size() / 2f, scale, SpriteEffects.None, 0f);
		spriteBatch.Draw(glowTex, position, null, Color.White * 0.45f * color.A / 255f, 0f, glowTex.Size() / 2f, scale * 0.45f, SpriteEffects.None, 0f);
	}

	private static void GetConstellationPalette(bool fullyAllocated, bool highlighted, float pulse, out Color coreColor, out Color glowColor, out Color nodeColor)
	{
		if (fullyAllocated)
		{
			coreColor = Color.Lerp(new Color(165, 220, 255), new Color(225, 245, 255), 0.5f);
			glowColor = new Color(95, 175, 255);
			nodeColor = new Color(210, 240, 255);
			return;
		}

		if (highlighted)
		{
			coreColor = Color.Lerp(new Color(120, 160, 220), new Color(200, 235, 255), pulse * 0.7f);
			glowColor = Color.Lerp(new Color(70, 120, 200), new Color(120, 200, 255), pulse);
			nodeColor = Color.Lerp(new Color(160, 210, 255), Color.White, pulse * 0.6f);
			return;
		}

		coreColor = new Color(80, 110, 155);
		glowColor = new Color(45, 70, 120);
		nodeColor = new Color(110, 150, 200);
	}

	/// <summary><inheritdoc cref="SmartUiElement.SafeUpdate(GameTime)"/><br/>
	/// Also handles repositioning by dragging the cursor </summary>
	public override void SafeUpdate(GameTime gameTime)
	{
		// Smoothly interpolate zoom and drag center towards targets
		float oldZoom = Zoom;
		Zoom = MathUtils.Damp(Zoom, TargetZoom, 0.02f, (float)gameTime.ElapsedGameTime.TotalSeconds);
		DragCenter = MathUtils.Damp(DragCenter, _targetDragCenter, 0.02f, (float)gameTime.ElapsedGameTime.TotalSeconds);

		// If zoom changed due to interpolation, update element positions
		if (Math.Abs(Zoom - oldZoom) > 0.001f)
		{
			foreach (UIElement c in _draggable)
			{
				if (!_origPositions.TryGetValue(c, out (float OrigLeft, float OrigTop, float OrigWidth, float OrigHeight) orig))
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

		DragPanel();
	}

	private void DragPanel()
	{
		Vector2 oldOffset = DragOffset;
		MouseState state = Mouse.GetState();
		DragOffset = new Vector2(state.X, state.Y);

		//Manually check mouse input because other elements shouldn't be allowed to interfere
		if (ContainsPoint(Main.MouseScreen) && Main.mouseLeft)
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
		if (!EnableZoom || !ContainsPoint(Main.MouseScreen) || ItemSlot.ShiftInUse)
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
