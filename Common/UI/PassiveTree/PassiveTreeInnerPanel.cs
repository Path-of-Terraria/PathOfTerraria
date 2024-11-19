using PathOfTerraria.Common.Systems.PassiveTreeSystem;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.PassiveTree;

internal class PassiveTreeInnerPanel : SmartUiElement
{
	private Vector2 _start;
	private Vector2 _lineOff;

	private UIElement Panel => Parent;

	private PassiveTreePlayer PassiveTreeSystem => Main.LocalPlayer.GetModPlayer<PassiveTreePlayer>();
	private TreeState UiTreeState => SmartUiLoader.GetUiState<TreeState>();
	public override string TabName => "PassiveTree";

	public override void Draw(SpriteBatch spriteBatch)
	{
		Rectangle oldRect = spriteBatch.GraphicsDevice.ScissorRectangle;
		spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
		spriteBatch.GraphicsDevice.ScissorRectangle = Panel.GetDimensions().ToRectangle();

		spriteBatch.End();
		spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

		foreach (PassiveEdge edge in PassiveTreeSystem.Edges)
		{
			Texture2D chainTex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Link").Value;

			Color color = Color.Gray;

			if (edge.End.CanAllocate(Main.LocalPlayer) && edge.Start.Level > 0)
			{
				color = Color.Lerp(Color.Gray, Color.White,
					(float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);
			}

			if (edge.End.Level > 0 && edge.Start.Level > 0)
			{
				color = Color.White;
			}

			for (float k = 0; k <= 1; k += 1 / (Vector2.Distance(edge.Start.TreePos, edge.End.TreePos) / 16))
			{
				Vector2 pos = GetDimensions().Center() + Vector2.Lerp(edge.Start.TreePos, edge.End.TreePos, k) +
				              _lineOff;
				Main.spriteBatch.Draw(chainTex, pos, null, color,
					edge.Start.TreePos.DirectionTo(edge.End.TreePos).ToRotation(), chainTex.Size() / 2, 1, 0, 0);
			}

			if (edge.End.Level > 0 && edge.Start.Level > 0)
			{
				Texture2D glow = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/GlowAlpha")
					.Value;
				var glowColor = new Color(255, 230, 150) { A = 0 };

				var rand = new Random(edge.GetHashCode());

				for (int k = 0; k < 8; k++)
				{
					float dist = Vector2.Distance(edge.Start.TreePos, edge.End.TreePos);
					float len = (40 + rand.Next(120)) * dist / 50;
					float scale = 0.05f + rand.NextSingle() * 0.15f;

					float progress = (Main.GameUpdateCount + 15 * k) % len / len;
					Vector2 pos = GetDimensions().Center() +
					              Vector2.SmoothStep(edge.Start.TreePos, edge.End.TreePos, progress) + _lineOff;
					float scale2 = (float)Math.Sin(progress * 3.14f) * (0.4f - scale);
					spriteBatch.Draw(glow, pos, null, glowColor * scale2, 0, glow.Size() / 2f, scale2, 0, 0);
				}
			}
		}

		base.Draw(spriteBatch);

		spriteBatch.End();
		spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

		spriteBatch.GraphicsDevice.ScissorRectangle = oldRect;
		spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = false;
	}

	public override void SafeUpdate(GameTime gameTime)
	{
		Vector2 offsetChange = Vector2.Zero;

		if (MouseContained.Left)
		{
			if (_start == Vector2.Zero)
			{
				_start = Main.MouseScreen;
			}

			offsetChange = Main.MouseScreen - _start;
			_start = Main.MouseScreen;
		}
		else
		{
			_start = Vector2.Zero;
		}

		Rectangle rec = GetDimensions().ToRectangle();
		Vector2 adjust = Vector2.Zero;
		Vector2 newOffset = _lineOff + offsetChange;

		float xAbove = newOffset.X + rec.Width / 2 + UiTreeState.TopLeftTree.X;
		float yAbove = newOffset.Y + rec.Height / 2 + UiTreeState.TopLeftTree.Y;

		float xBelow = newOffset.X - rec.Width / 2 + UiTreeState.BotRightTree.X;
		float yBelow = newOffset.Y - rec.Height / 2 + UiTreeState.BotRightTree.Y;

		if (rec.Height < MathF.Abs(UiTreeState.BotRightTree.Y) + MathF.Abs(UiTreeState.TopLeftTree.Y))
		{
			yAbove = MathF.Max(yAbove, 0);
			yBelow = MathF.Min(yBelow, 0);
		}
		else
		{
			yAbove = MathF.Min(yAbove, 0);
			yBelow = MathF.Max(yBelow, 0);
		}

		if (rec.Width < MathF.Abs(UiTreeState.BotRightTree.X) + MathF.Abs(UiTreeState.TopLeftTree.X))
		{
			xAbove = MathF.Max(xAbove, 0);
			xBelow = MathF.Min(xBelow, 0);
		}
		else
		{
			xAbove = MathF.Min(xAbove, 0);
			xBelow = MathF.Max(xBelow, 0);
		}

		adjust += new Vector2(xAbove, yAbove);
		adjust += new Vector2(xBelow, yBelow);

		offsetChange -= adjust;

		foreach (UIElement element in Elements)
		{
			if (element is PassiveElement ele)
			{
				element.Left.Set(ele.Left.Pixels + offsetChange.X, 0.5f);
				element.Top.Set(ele.Top.Pixels + offsetChange.Y, 0.5f);
			}
		}

		_lineOff += offsetChange;

		Recalculate();
	}
}