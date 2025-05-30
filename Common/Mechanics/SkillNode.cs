using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Common.Mechanics;

public abstract class SkillNode(SkillTree tree) : Allocatable
{
	public SkillTree Tree => tree;

	public override bool CanAllocate(Player player)
	{
		return Connected();

		bool Connected()
		{
			bool value = true;

			foreach (SkillTree.Edge edge in Tree.Edges)
			{
				if (edge.Contains(this))
				{
					if (edge.Other(this) is not SkillPassive p || p.Level > 0)
					{
						return true;
					}

					value = false;
				}
			}

			return value;
		}
	}
}