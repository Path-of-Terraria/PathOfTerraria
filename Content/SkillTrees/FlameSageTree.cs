using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.SkillPassives;
using PathOfTerraria.Content.SkillPassives.FlameSage;
using PathOfTerraria.Content.Skills.Summon;
using PathOfTerraria.Content.SkillSpecials.FlameSageSpecials;

namespace PathOfTerraria.Content.SkillTrees;

internal class FlameSageTree : SkillTree
{
	public override Type ParentSkill => typeof(FlameSage);

	public override void Populate()
	{
		var anchor = new Anchor(this) { Level = 1 };
		var flamethrower = new Flamethrower(this) { TreePos = new Vector2(-200, 0) };
		var sentinel = new MoltenSentinel(this) { TreePos = new Vector2(0, 200) };
		var intenseHeat = new IntenseHeat(this) { TreePos = new Vector2(-100, 200) };
		var flameWard = new FlameWard(this) { TreePos = new Vector2(100, 200) };
		var livingFurnace = new LivingFurnace(this) { TreePos = new Vector2(0, 300) };

		AddNodes(anchor, flamethrower, sentinel);
		AddNodes(sentinel, intenseHeat, flameWard, livingFurnace);
	}
}