using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI;

internal abstract class AllocatableInnerPanel : SmartUiElement
{
	private readonly HashSet<UIElement> _draggable = [];

	protected Vector2 DragOffset;
	protected Vector2 DragCenter;

	public List<Edge<AllocatableElement>> Connections { get; } = [];

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
	}

	protected override void DrawChildren(SpriteBatch spriteBatch)
	{
		// Drawing connections here means it gets clipped by OverflowHidden correctly
		DrawEdgeConnections(spriteBatch, CollectionsMarshal.AsSpan(Connections));

		base.DrawChildren(spriteBatch);
	}

	public static void DrawEdgeConnections(SpriteBatch spriteBatch, ReadOnlySpan<Edge<AllocatableElement>> connections)
	{
		Vector2 center = default;
		Texture2D chainTex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Link").Value;

		foreach (Edge<AllocatableElement> edge in connections)
		{
			Color color = Color.Gray;
			AllocatableElement start = edge.Start;
			AllocatableElement end = edge.End;

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

			Vector2 startPos = start.GetDimensions().Center();
			Vector2 endPos = end.GetDimensions().Center();

			if (!edge.Flags.HasFlag(EdgeFlags.EffectsOnly))
			{
				for (float k = 0; k <= 1; k += 1 / (Vector2.Distance(startPos, endPos) / 16))
				{
					Vector2 pos = center + Vector2.Lerp(startPos, endPos, k);
					spriteBatch.Draw(chainTex, pos, null, color, startPos.DirectionTo(endPos).ToRotation(), chainTex.Size() / 2, 1, 0, 0);
				}
			}

			bool showParticles = start.AppearsAsAllocated() && (end.AppearsAsAllocated() || (edge.Flags.HasFlag(EdgeFlags.EffectsOnly) && end.AppearsAsCanBeAllocated()));

			if (showParticles)
			{
				Texture2D glow = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/GlowAlpha").Value;
				var glowColor = new Color(255, 230, 150) { A = 0 };

				var rand = new Random(edge.GetHashCode());

				for (int k = 0; k < 8; k++)
				{
					float dist = Vector2.Distance(startPos, endPos);
					float len = (40 + rand.Next(120)) * dist / 50;
					float scale = 0.05f + rand.NextSingle() * 0.15f;

					float progress = (Main.GameUpdateCount + 15 * k) % len / len;
					Vector2 pos = center + Vector2.SmoothStep(startPos, endPos, progress);
					float scale2 = (float)Math.Sin(progress * 3.14f) * (0.4f - scale);

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
}