using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Common.Mechanics;

public abstract class SkillNode(SkillTree tree) : Allocatable
{
	public SkillTree Tree => tree;

	public override bool CanAllocate(Player player)
	{
		return base.CanAllocate(player) && Connections > 0;
	}

	public override bool CanDeallocate(Player player)
	{
		return base.CanDeallocate(player) && Connections < 2;
	}

	private int Connections
	{
		get
		{
			int count = 0;
			foreach (SkillTree.Edge edge in Tree.Edges)
			{
				if (edge.Contains(this) && edge.Other(this).Allocated)
				{
					count++;
				}
			}

			return count;
		}
	}
}