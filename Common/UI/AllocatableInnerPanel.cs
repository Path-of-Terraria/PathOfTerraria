using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Core.UI.SmartUI;
using System.Collections.Generic;
using Terraria.UI;

namespace PathOfTerraria.Common.UI;

internal abstract class AllocatableInnerPanel : SmartUiElement
{
	public abstract IEnumerable<Edge> Connections { get; }
	private readonly HashSet<UIElement> _draggable = [];

	protected Vector2 DragOffset;
	protected Vector2 DragCenter;

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
		Vector2 center = GetDimensions().Center() + DragCenter;

		foreach (Edge edge in Connections) //Drawing connections here means it gets clipped by OverflowHidden correctly
		{
			if (edge.Start == null || edge.End == null)
			{
				continue;
			}
			
			Texture2D chainTex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Link").Value;
			Color color = Color.Gray;

			Allocatable start = edge.Start;
			Allocatable end = edge.End;

			if (end.CanAllocate(Main.LocalPlayer) && end.Allocated)
			{
				color = Color.Lerp(Color.Gray, Color.White,
					(float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);
			}

			if (end.Allocated && start.Allocated)
			{
				color = Color.White;
			}

			for (float k = 0; k <= 1; k += 1 / (Vector2.Distance(start.TreePos, end.TreePos) / 16))
			{
				Vector2 pos = center + Vector2.Lerp(start.TreePos, end.TreePos, k);
				Main.spriteBatch.Draw(chainTex, pos, null, color, start.TreePos.DirectionTo(end.TreePos).ToRotation(), chainTex.Size() / 2, 1, 0, 0);
			}

			if (end.Allocated && start.Allocated)
			{
				Texture2D glow = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/GlowAlpha").Value;
				var glowColor = new Color(255, 230, 150) { A = 0 };

				var rand = new Random(edge.GetHashCode());

				for (int k = 0; k < 8; k++)
				{
					float dist = Vector2.Distance(start.TreePos, end.TreePos);
					float len = (40 + rand.Next(120)) * dist / 50;
					float scale = 0.05f + rand.NextSingle() * 0.15f;

					float progress = (Main.GameUpdateCount + 15 * k) % len / len;
					Vector2 pos = center + Vector2.SmoothStep(start.TreePos, end.TreePos, progress);
					float scale2 = (float)Math.Sin(progress * 3.14f) * (0.4f - scale);

					spriteBatch.Draw(glow, pos, null, glowColor * scale2, 0, glow.Size() / 2f, scale2, 0, 0);
				}
			}
		}

		base.DrawChildren(spriteBatch);
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