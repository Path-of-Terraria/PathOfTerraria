using PathOfTerraria.Common.Mechanics;

namespace PathOfTerraria.Common.UI.SkillsTree;

internal class SkillSpecialElement : SkillElement
{
	public SkillSpecial Special { get; }

	public SkillSpecialElement(SkillSpecial node) : base(node)
	{
		Special = node;
	}

	public override void DrawNode(Allocatable node, SpriteBatch spriteBatch, Vector2 center)
	{
		var special = (SkillSpecial)node;
		Texture2D texture = node.Texture.Value;
		Color color = Color.Gray;

		if (AppearsAsCanBeAllocated(node))
		{
			color = Color.Lerp(Color.Gray, Color.White, (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);
		}

		if (special.Tree.Specialization == special)
		{
			color = Color.White;
		}

		spriteBatch.Draw(texture, center, null, color, 0, texture.Size() / 2, 1, default, 0);

		if (special.Tree.Specialization == special)
		{
			float pulse = (float)Math.Sin(Main.timeForVisualEffects / 50f) * .25f + .5f;

			Texture2D glow = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/GlowAlpha").Value;
			var glowColor = new Color(255, 230, 150) { A = 0 };
			spriteBatch.Draw(glow, center, null, glowColor * pulse, 0, glow.Size() / 2, 1, default, 0);

			Texture2D outline = ModContent.Request<Texture2D>($"{PoTMod.ModName}/Assets/UI/SpecialFrame_Outline").Value;
			spriteBatch.Draw(outline, center, null, Color.White * pulse, 0, outline.Size() / 2, 1, default, 0);
		}
	}
}
