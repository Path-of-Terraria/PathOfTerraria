using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.SkillPassives;
using PathOfTerraria.Content.SkillPassives.SwarmPassives;
using PathOfTerraria.Content.Skills.Summon;
using PathOfTerraria.Content.SkillSpecials.PestSwarmSpecials;

namespace PathOfTerraria.Content.SkillTrees;

internal class PestSwarmTree : SkillTree
{
	public override Type ParentSkill => typeof(PestSwarm);

	public override void Populate()
	{
		var anchor = new Anchor(this) { Level = 1 };
		var swarm = new AntlionSwarm(this) { TreePos = new Vector2(200, 0) };
		var locusts = new LocustBrood(this) { TreePos = new Vector2(0, -200) };
		
		AddNodes(anchor, swarm, locusts);

		var brood = new BiggerBrood(this) { TreePos = new Vector2(200, -100) };

		AddNodes(locusts, brood);
	}
}