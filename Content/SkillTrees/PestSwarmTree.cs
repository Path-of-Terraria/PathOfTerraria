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
		
		AddNodes(anchor, swarm);

		//var brood = new BiggerBrood(this) { TreePos = new Vector2(200, -100) };

		//AddNodes(swarm, brood);
	}
}