using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.UI.Utilities;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.SkillsTree;

internal class SkillTreeInnerPanel : AllocatableInnerPanel
{
	public override string TabName => "SelectedSkillTree";

	private readonly Skill _skill;

	public SkillTreeInnerPanel(Skill skill) : base()
	{
		_skill = skill;
		Width = Height = StyleDimension.Fill;
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		AvailablePassivePointsText.DrawAvailablePassivePoint(spriteBatch, _skill.Tree.Points, GetRectangle().TopLeft() + new Vector2(35, 35));
		base.Draw(spriteBatch);
	}
}