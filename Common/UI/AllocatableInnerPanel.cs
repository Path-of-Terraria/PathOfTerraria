using System.Collections.Generic;
using System.Runtime.InteropServices;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Core.UI.SmartUI;
using ReLogic.Content;
using Terraria.UI;
using Terraria.Utilities;

namespace PathOfTerraria.Common.UI;

internal abstract class AllocatableInnerPanel : SmartUiElement
{
	private static readonly Asset<Texture2D> _chainTex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Link");

	private readonly HashSet<UIElement> _draggable = [];
	private readonly Dictionary<UIElement, (float OrigLeft, float OrigTop, float OrigWidth, float OrigHeight)> _origPositions = [];

	private const float ZoomMin = 0.5f;
	private const float ZoomMax = 2f;
	private const float ZoomStep = 0.1f;

	/// <summary> Whether this panel supports scroll-wheel zoom. Override in a subclass to enable. </summary>
	protected virtual bool EnableZoom => false;

	/// <summary> Current zoom level. 1 is the default; range is [<see cref="ZoomMin"/>, <see cref="ZoomMax"/>]. </summary>
	public float Zoom { get; private set; } = 1f;

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

		if (ContainsPoint(Main.MouseScreen) && Main.mouseLeft) //Manually check mouse input because other elements shouldn't be allowed to interfere
		{
			Vector2 velocity = DragOffset - oldOffset;
			DragCenter += velocity;

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

		float oldZoom = Zoom;
		float delta = evt.ScrollWheelValue > 0 ? ZoomStep : -ZoomStep;
		float newZoom = Math.Clamp(oldZoom + delta, ZoomMin, ZoomMax);

		if (newZoom == oldZoom)
		{
			return;
		}

		// Zoom toward the mouse cursor so the point under the cursor stays fixed.
		CalculatedStyle dims = GetDimensions();
		var panelCenter = new Vector2(dims.X + dims.Width * 0.5f, dims.Y + dims.Height * 0.5f);
		Vector2 mouseRelCenter = Main.MouseScreen - panelCenter;

		float ratio = newZoom / oldZoom;
		DragCenter = mouseRelCenter * (1f - ratio) + DragCenter * ratio;

		foreach (UIElement c in _draggable)
		{
			if (!_origPositions.TryGetValue(c, out var orig))
			{
				continue;
			}

			c.Left.Pixels = orig.OrigLeft * newZoom + DragCenter.X;
			c.Top.Pixels = orig.OrigTop * newZoom + DragCenter.Y;
			c.Width.Pixels = orig.OrigWidth * newZoom;
			c.Height.Pixels = orig.OrigHeight * newZoom;
		}

		Zoom = newZoom;
		Recalculate();
	}
}