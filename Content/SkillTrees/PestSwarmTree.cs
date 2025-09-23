using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.SkillPassives;
using PathOfTerraria.Content.SkillPassives.FlameSage;
using PathOfTerraria.Content.Skills.Summon;
using PathOfTerraria.Content.SkillSpecials.FlameSageSpecials;

namespace PathOfTerraria.Content.SkillTrees;

internal class PestSwarmTree : SkillTree
{
	public override Type ParentSkill => typeof(PestSwarm);

	public override void Populate()
	{
		var anchor = new Anchor(this) { Level = 1 };

		AddNodes(anchor);
	}
}