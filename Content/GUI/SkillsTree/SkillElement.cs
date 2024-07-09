using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Mechanics;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace PathOfTerraria.Content.GUI.SkillsTree;

internal class SkillElement : UIElement
{
	private readonly Skill _skill;
	
	public SkillElement(Skill skill)
	{
		_skill = skill;
		Width.Set(skill.Size.X, 0);
		Height.Set(skill.Size.Y, 0);
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		Texture2D tex = ModContent.Request<Texture2D>(_skill.Texture).Value;

		if (tex == null)
		{
			return;
		}

		Vector2 position = GetDimensions().Position() + new Vector2(Width.Pixels / 2, Height.Pixels / 2);
		spriteBatch.Draw(tex, position, null, Color.White, 0f, tex.Size() / 2f, 1f, SpriteEffects.None, 0f);
	}
	
	public override void LeftClick(UIMouseEvent evt)
	{
		Main.NewText("Clicked on " + _skill.Name);
	}

	public override void RightClick(UIMouseEvent evt)
	{
		Main.NewText("Right Clicked on " + _skill.Name);
	}
}