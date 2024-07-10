using PathOfTerraria.Core.Loaders.UILoading;
using PathOfTerraria.Core.Mechanics;

namespace PathOfTerraria.Content.GUI.SkillsTree;

internal class SkillsTreeInnerPanel : SmartUIElement
{
	public override string TabName => "SkillTree";

	private bool _drewSkills;
	
	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);
		if (!_drewSkills)
		{
			_drewSkills = true;
			AppendAllSkills();
		}
	}

	private void AppendAllSkills()
	{
		int index = 0;
		foreach (Type type in PathOfTerraria.Instance.Code.GetTypes())
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(Skill)))
			{
				continue;
			}
			
			var element = new SkillElement((Skill)Activator.CreateInstance(type), index);
			Append(element);
			index += 1;
		}
	}
}