using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Content.SkillPassives;

namespace PathOfTerraria.Common.UI.SkillsTree;

internal class SkillElement : AllocatableElement
{
	public SkillNode Skill { get; }

	public SkillElement(SkillNode node) : base(node)
	{
		Skill = node;

		if (Node is Anchor)
		{
			(Node as SkillPassive).Level = 1;
		}
	}
}
