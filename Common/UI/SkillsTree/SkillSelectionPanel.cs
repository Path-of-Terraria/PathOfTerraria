using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Core.UI.SmartUI;
using Terraria.ModLoader.Core;

namespace PathOfTerraria.Common.UI.SkillsTree;

internal class SkillSelectionPanel : SmartUiElement
{
	private SkillTreeInnerPanel _skillTreeInnerPanel;
	public override string TabName => "SkillTree";

	private bool _drewSkills;
	public Skill SelectedSkill { get; set; }
	
	public override void Draw(SpriteBatch spriteBatch)
	{
		base.Draw(spriteBatch);

		if (!_drewSkills && SelectedSkill == null)
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

			var skill = (Skill) Activator.CreateInstance(type);
			var element = new SkillSelectionElement(skill, index, this);
			Append(element);
			index += 1;
		}
	}

	public void RebuildTree()
	{
		if (SelectedSkill == null)
		{
			return;
		}
		
		if (SelectedSkill.Passives.Count == 0)
		{
			Main.NewText("This skill has no passives.");
			return;
		}
		
		RemoveAllChildren();
		_skillTreeInnerPanel = null;
		_skillTreeInnerPanel = new SkillTreeInnerPanel(SelectedSkill);
		Append(_skillTreeInnerPanel);
		SelectedSkill.CreateTree();
		foreach (SkillPassive n in SelectedSkill.ActiveNodes)
		{
			_skillTreeInnerPanel.Append(new SkillPassiveElement(n));
		}
	}
}