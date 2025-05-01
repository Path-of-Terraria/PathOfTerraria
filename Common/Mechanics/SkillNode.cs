using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Common.Mechanics;

public abstract class SkillNode(SkillTree tree) : Allocatable
{
	public SkillTree Tree => tree;
}