using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.ModLoader.Core;

namespace PathOfTerraria.Common.UI.SkillsTree;

internal class SkillSelectionPanel : SmartUiElement
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
		foreach (Type type in AssemblyManager.GetLoadableTypes(PoTMod.Instance.Code))
		{
			if (type.IsAbstract || !type.IsSubclassOf(typeof(Skill)))
			{
				continue;
			}
			
			var element = new SkillSelectionElement((Skill)Activator.CreateInstance(type), index);
			Append(element);
			index += 1;
		}
	}
}