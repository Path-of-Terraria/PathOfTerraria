using System.Collections.Generic;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.SkillAugments;
using PathOfTerraria.Core.UI.SmartUI;
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

		_skillTreeInnerPanel = new SkillTreeInnerPanel(SelectedSkill);
		Append(_skillTreeInnerPanel);

		List<SkillNode> list = SkillTree.Current.Nodes;
		int spareSlotCounter = 0;
		int count = Math.Min(SkillTree.DefaultAugmentCount, SkillTree.Current.Augments.Count);

		foreach (SkillNode node in list)
		{
			UIElement element = node switch
			{
				SpareSlot => new AugmentSlotElement(spareSlotCounter++, true, node),
				_ => new SkillElement(node),
			};
			element.Left.Set(node.TreePos.X - node.Size.X / 2, 0.5f);
			element.Top.Set(node.TreePos.Y - node.Size.Y / 2, 0.5f);

			_skillTreeInnerPanel.AppendAsDraggable(element);
		}

		for (int i = 0; i < count; i++)
		{
			_skillTreeInnerPanel.Append(new AugmentSlotElement(spareSlotCounter++, SkillTree.Current.Augments[i].Unlocked));
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
			_skillTreeInnerPanel = new SkillTreeInnerPanel(SelectedSkill);

			AppendAllSkills();
		};
		Append(closeButton);
	}
}