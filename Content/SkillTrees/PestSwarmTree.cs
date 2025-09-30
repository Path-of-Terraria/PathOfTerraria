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

		var brood = new BiggerBrood(this) { TreePos = new Vector2(0, -300), MaxLevel = 2 };
		var gest = new Gestation(this) { TreePos = new Vector2(100, -200) };
		var egg = new Eggsplosion(this) { TreePos = new Vector2(-100, -200) };
		AddNodes(locusts, brood, gest, egg);
		AddNodes(egg, new ShockingEmergence(this) { TreePos = new Vector2(-100, -100) });
		AddNodes(gest, new QuickerHatching(this) { TreePos = new Vector2(200, -200), MaxLevel = 2 }, new CarnivorousLarvae(this) { TreePos = new Vector2(100, -300) });

		var vola = new VolatileInsects(this) { TreePos = new Vector2(300, 0) };
		AddNodes(swarm, vola);
		AddNodes(swarm, new ViciousBites(this) { TreePos = new Vector2(200, -100) });

		var inf = new InfectedDetonation(this) { TreePos = new Vector2(400, 0) };
		AddNodes(vola, inf);
	}
}