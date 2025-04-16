using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.UI.Utilities;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.SkillsTree;

internal class SkillTreeInnerPanel : SmartUiElement
{
	private UIElement Panel => Parent;
	private readonly SkillTree _tree;

	public override string TabName => "SelectedSkillTree";

	public SkillTreeInnerPanel(SkillTree tree)
	{
		_tree = tree;
		Height = StyleDimension.FromPixels(768);
		Width = StyleDimension.FromPixels(900);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Rectangle oldRect = spriteBatch.GraphicsDevice.ScissorRectangle;
		spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
		spriteBatch.GraphicsDevice.ScissorRectangle = Panel.GetDimensions().ToRectangle();
		
		int availablePoints = _tree.Points;
		AvailablePassivePointsText.DrawAvailablePassivePoint(spriteBatch, availablePoints, GetRectangle().TopLeft() + new Vector2(35, 35));

		spriteBatch.End();
		spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

		foreach (SkillTree.Edge edge in _tree.Edges)
		{
			Texture2D chainTex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Link").Value;
			Color color = Color.Gray;

			int startLevel = (edge.Start is SkillPassive s) ? s.Level : 1;
			int endLevel = (edge.End is SkillPassive e) ? e.Level : 1;

			Vector2 startPos = _tree.Point(edge.Start);
			Vector2 endPos = _tree.Point(edge.End);

			if (edge.End.CanAllocate(Main.LocalPlayer) && startLevel > 0)
			{
				color = Color.Lerp(Color.Gray, Color.White, (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);
			}

			if (endLevel > 0 && startLevel > 0)
			{
				color = Color.White;
			}

			for (float k = 0; k <= 1; k += 1 / (Vector2.Distance(startPos, endPos) / 16))
			{
				Vector2 pos = GetDimensions().Center() + Vector2.Lerp(startPos, endPos, k);
				Main.spriteBatch.Draw(chainTex, pos, null, color,
					startPos.DirectionTo(endPos).ToRotation(), chainTex.Size() / 2, 1, 0, 0);
			}

			if (endLevel > 0 && startLevel > 0)
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
					Vector2 pos = GetDimensions().Center() + Vector2.SmoothStep(startPos, endPos, progress);
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
}