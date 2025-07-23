using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.SkillPassives;
using PathOfTerraria.Content.SkillPassives.RainOfArrowsTree;
using PathOfTerraria.Content.Skills.Ranged;
using PathOfTerraria.Content.SkillSpecials.RainOfArrowsSpecials;
using Terraria.GameContent;

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
		AddNodes(natures, spores, linger, new CreepingVines(this) { TreePos = new Vector2(-300, 0) });
		AddNodes(spores, new MoldColony(this) { TreePos = new Vector2(-300, -100), MaxLevel = 2 }, 
			new FungalSpread(this) { TreePos = new Vector2(-200, -200), MaxLevel = 2 });
		AddNodes(linger, new PowerfulSmog(this) { TreePos = new Vector2(-200, 200), MaxLevel = 2 }, 
			new Megatoxin(this) { TreePos = new Vector2(-300, 100), MaxLevel = 2 });

		var explosive = new ExplosiveVolley(this) { TreePos = new Vector2(0, 100) };
		var shattering = new ShatteringArrows(this) { TreePos = new Vector2(0, 200) };
		AddNodes(anchor, explosive);
		AddNodes(explosive, new ColdBlast(this) { TreePos = new Vector2(100, 100) }, shattering);
		AddNodes(shattering, new ConcussiveBurst(this) { TreePos = new Vector2(-100, 200), MaxLevel = 3 }, new SlicingShrapnel(this) { TreePos = new Vector2(0, 300) });

		var precision = new PiercingPrecision(this) { TreePos = new Vector2(200, 0) };
		var sharp = new SharpenedTips(this) { TreePos = new Vector2(300, 0), MaxLevel = 2 };
		AddNodes(anchor, precision);
		AddNodes(precision, new Quickload(this) { TreePos = new Vector2(200, -100) }, sharp);
		AddNodes(sharp, new TargetLock(this) { TreePos = new Vector2(300, 100) }, new Ghostfire(this) { TreePos = new Vector2(400, 0) });
	}
}
