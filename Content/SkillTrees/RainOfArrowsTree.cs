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
		var natures = new NaturesBarrage(this) { TreePos = new Vector2(-250, 0) };
		var spores = new FesteringSpores(this) { TreePos = new Vector2(-250, -100) };

		AddNodes(anchor, natures);
		AddNodes(natures, spores);
		AddNodes(spores, new MoldColony(this) { TreePos = new Vector2(-350, -100), MaxLevel = 2 });
	}
}
