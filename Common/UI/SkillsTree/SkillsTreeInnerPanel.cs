using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Core.UI;
using Terraria.ModLoader.Core;

namespace PathOfTerraria.Common.UI.SkillsTree;

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
		foreach (Type type in AssemblyManager.GetLoadableTypes(PathOfTerraria.Instance.Code))
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