using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Core.UI.SmartUI;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ModLoader.Core;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace PathOfTerraria.Common.UI.SkillsTree;

internal class SkillSelectionPanel : SmartUiElement
{
	public override string TabName => "SkillTree";

	public Skill SelectedSkill { get; set; }

	private bool _drewSkills;
	private SkillTreeInnerPanel _skillTreeInnerPanel;

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
		if (SelectedSkill is null || SelectedSkill.Tree is null)
		{
			return;
		}

		SkillTree.Current = SelectedSkill.Tree;
		RemoveAllChildren();
		_skillTreeInnerPanel = null;
		_skillTreeInnerPanel = new SkillTreeInnerPanel(SelectedSkill);
		Append(_skillTreeInnerPanel);

		Dictionary<Vector2, SkillNode> dict = SkillTree.Current.Nodes;
		foreach (Vector2 key in dict.Keys)
		{
			_skillTreeInnerPanel.Append(new AllocatableElement(key, dict[key]));
		}

		int augmentSlots = SelectedSkill.Tree.Augments.Length;
		for (int i = 0; i < augmentSlots; i++)
		{
			_skillTreeInnerPanel.Append(new AugmentSlotElement(i));
		}

		UIButton<string> closeButton = new(Language.GetTextValue("Mods.PathOfTerraria.UI.SkillUI.Back"))
		{
			Width = StyleDimension.FromPixels(80),
			Height = StyleDimension.FromPixels(30),
			Top = StyleDimension.FromPixels(64),
			Left = StyleDimension.FromPixels(10)
		};

		closeButton.OnLeftClick += (_, _) =>
		{
			RemoveAllChildren();

			SelectedSkill = null;
			_skillTreeInnerPanel = null;
			_skillTreeInnerPanel = new SkillTreeInnerPanel(SelectedSkill);

			AppendAllSkills();
		};
		Append(closeButton);
	}
}