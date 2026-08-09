using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.ModPlayers.SkillPlayers;
using PathOfTerraria.Content.SkillPassives;
using System.Collections.Generic;

namespace PathOfTerraria.Common.Systems.Skills;

/// <summary>
/// Public entry-point for collecting a serializable snapshot of the player's equipped skills
/// and their skill trees. Lets external mods (e.g. PathOfTerrariaOnline) read tree data without
/// needing access to internal types like <see cref="SkillCombatPlayer"/> or <see cref="Edge{T}"/>.
/// </summary>
public static class SkillsSnapshotProvider
{
	public static SkillsSnapshot Build(Player player)
	{
		SkillCombatPlayer combat = player.GetModPlayer<SkillCombatPlayer>();
		var skills = new List<EquippedSkillSnapshot>();

		for (int i = 0; i < combat.HotbarSkills.Length; i++)
		{
			Skill skill = combat.HotbarSkills[i];

			if (skill is null)
			{
				continue;
			}

			SkillTree tree = skill.Tree;
			var nodes = new List<SkillTreeNodeSnapshot>();
			var edges = new List<SkillTreeEdgeSnapshot>();

			if (tree is not null)
			{
				foreach (SkillNode node in tree.Nodes)
				{
					nodes.Add(new SkillTreeNodeSnapshot
					{
						InternalIdentifier = node.Name,
						DisplayName = node.DisplayName ?? node.Name,
						DisplayTooltip = node.DisplayTooltip,
						PosX = node.TreePos.X,
						PosY = node.TreePos.Y,
						Level = node.Level,
						MaxLevel = node.MaxLevel,
						IsSpecialization = node is SkillSpecial,
						IsAnchor = node is Anchor,
						IsHidden = node.IsHidden,
					});
				}

				foreach (Edge<Allocatable> edge in tree.Edges)
				{
					if (edge.Start is not SkillNode start || edge.End is not SkillNode end)
					{
						continue;
					}

					edges.Add(new SkillTreeEdgeSnapshot
					{
						FromInternalIdentifier = start.Name,
						ToInternalIdentifier = end.Name,
					});
				}
			}

			bool sameAssembly = skill.GetType().Assembly == typeof(SkillsSnapshotProvider).Assembly;

			skills.Add(new EquippedSkillSnapshot
			{
				SlotIndex = i,
				SourceMod = sameAssembly ? PoTMod.ModName : skill.GetType().Assembly.GetName().Name,
				InternalName = skill.Name,
				DisplayName = skill.DisplayName.Value,
				Level = skill.Level,
				MaxLevel = skill.MaxLevel,
				Points = tree?.Points ?? 0,
				SpecializationInternalName = tree?.Specialization?.Name,
				SpecializationDisplayName = tree?.Specialization?.DisplayName,
				Nodes = nodes,
				Edges = edges,
			});
		}

		return new SkillsSnapshot { Skills = skills };
	}
}
