using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.SkillsTree;

internal class SkillTreeInnerPanel(Skill skill) : SmartUiElement
{
	private UIElement Panel => Parent;
	public override string TabName => "SelectedSkillTree";

	public override void Draw(SpriteBatch spriteBatch)
	{
		Rectangle oldRect = spriteBatch.GraphicsDevice.ScissorRectangle;
		spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
		spriteBatch.GraphicsDevice.ScissorRectangle = Panel.GetDimensions().ToRectangle();

		spriteBatch.End();
		spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

		foreach (SkillPassiveEdge edge in skill.Edges)
		{
			Texture2D chainTex = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/Link").Value;

			Color color = Color.Gray;

			if (edge.End.CanAllocate() && edge.Start.Level > 0)
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
				Vector2 pos = GetDimensions().Center() + Vector2.Lerp(edge.Start.TreePos, edge.End.TreePos, k);
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
					              Vector2.SmoothStep(edge.Start.TreePos, edge.End.TreePos, progress);
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