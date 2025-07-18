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
		var natures = new NaturesBarrage(this) { TreePos = new Vector2(-200, 0) };
		var spores = new FesteringSpores(this) { TreePos = new Vector2(-200, -100) };
		var linger = new LingeringPoison(this) { TreePos = new Vector2(-200, 100) };

		AddNodes(anchor, natures);
		AddNodes(natures, spores);
		AddNodes(spores, new MoldColony(this) { TreePos = new Vector2(-300, -100), MaxLevel = 2 }, 
			new FungalSpread(this) { TreePos = new Vector2(-200, -200), MaxLevel = 2 });
		AddNodes(natures, linger);
		AddNodes(linger, new PowerfulSmog(this) { TreePos = new Vector2(-200, 200), MaxLevel = 2 }, 
			new Megatoxin(this) { TreePos = new Vector2(-300, 100), MaxLevel = 2 });
	}
}
