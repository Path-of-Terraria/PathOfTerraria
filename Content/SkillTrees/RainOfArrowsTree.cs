using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.SkillPassives;
using PathOfTerraria.Content.SkillPassives.RainOfArrowsTree;
using PathOfTerraria.Content.Skills.Ranged;

namespace PathOfTerraria.Content.SkillTrees;

internal class RainOfArrowsTree : SkillTree
{
	public override Type ParentSkill => typeof(RainOfArrows);

	public override void Populate()
	{
		var anchor = new Anchor(this) { Level = 1 };
		var natures = new NaturesBarrage(this) { TreePos = new Vector2(-100, 0) };

		AddNodes(anchor, natures);
	}
}
