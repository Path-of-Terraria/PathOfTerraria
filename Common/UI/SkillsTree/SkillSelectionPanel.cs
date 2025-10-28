using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.UI.Guide;
using PathOfTerraria.Content.SkillAugments;
using PathOfTerraria.Core.UI.SmartUI;
using System.Collections.Generic;
using Terraria.Localization;
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

		foreach (Skill skill in ModContent.GetContent<Skill>())
		{
			var element = new SkillSelectionElement(skill, (parent, self) =>
			{
				SkillCombatPlayer skillCombatPlayer = Main.LocalPlayer.GetModPlayer<SkillCombatPlayer>();

				if (skillCombatPlayer.TryAddSkill(skill))
				{
					var panel = parent as SkillSelectionPanel;
					panel.SelectedSkill = self.Skill;
					panel.RebuildTree();

					Main.LocalPlayer.GetModPlayer<TutorialPlayer>().TutorialChecks.Add(TutorialCheck.SelectedSkill);
				}
			});

			element.Top.Set(60, 0);
			element.Left.Set(25 + 75 * index, 0);

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

		SkillTree tree = SkillTree.Current = SelectedSkill.Tree;

		RemoveAllChildren();

		_skillTreeInnerPanel = new SkillTreeInnerPanel(SelectedSkill);
		Append(_skillTreeInnerPanel);

		int spareSlotCounter = 0;
		
		// Add nodes.
		var mapping = new Dictionary<Allocatable, IConnectedAllocatableNode>(capacity: tree.Nodes.Count);
		foreach (SkillNode node in tree.Nodes)
		{
			UIElement element = node switch
			{
				SpareSlot => new AugmentSlotElement(node, spareSlotCounter++, true),
				SkillSpecial special => new SkillSpecialElement(special),
				_ => new SkillElement(node),
			};
			element.Left.Set(node.TreePos.X - node.Size.X / 2, 0.5f);
			element.Top.Set(node.TreePos.Y - node.Size.Y / 2, 0.5f);

			mapping[node] = (IConnectedAllocatableNode)element;

			_skillTreeInnerPanel.AppendAsDraggable(element);
		}

		// Add edges.
		_skillTreeInnerPanel.Connections.EnsureCapacity(tree.Edges.Count);
		foreach (Edge<Allocatable> edge in tree.Edges)
		{
			if (mapping.TryGetValue(edge.Start, out IConnectedAllocatableNode uiStart)
			&& mapping.TryGetValue(edge.End, out IConnectedAllocatableNode uiEnd))
			{
				_skillTreeInnerPanel.Connections.Add(new(uiStart, uiEnd, edge.Flags));
			}
		}

		// Add additional augment slots to the side.
		int count = Math.Min(SkillTree.DefaultAugmentCount, tree.Augments.Count);
		for (int i = 0; i < count; i++)
		{
			_skillTreeInnerPanel.Append(new AugmentSlotElement(node: null, index: spareSlotCounter++, unlocked: tree.Augments[i].Unlocked));
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