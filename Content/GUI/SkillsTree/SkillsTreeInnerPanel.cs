using PathOfTerraria.Content.Skills.Melee;
using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Mechanics;

namespace PathOfTerraria.Content.GUI.SkillsTree;

internal class SkillsTreeInnerPanel : SmartUIElement
{
	public override string TabName => "SkillTree";
	private bool _drewSkills = false;

	public override void Draw(SpriteBatch spriteBatch)
	{
		Utils.DrawBorderStringBig(
			spriteBatch, 
			"Skills - Placeholder",
			GetRectangle().TopLeft() + new Vector2(138, 150),
			Color.White,
			0.6f,
			0.5f,
			0.35f);
		
		if (!_drewSkills)
		{
			_drewSkills = true;
			DrawAllSkills();
		}
	}

	private void DrawAllSkills()
	{
		foreach (Type type in PathOfTerraria.Instance.Code.GetTypes())
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(Skill)))
			{
				continue;
			}
			
			var element = new SkillElement((Skill)Activator.CreateInstance(type));
			Append(element);
		}
	}
}