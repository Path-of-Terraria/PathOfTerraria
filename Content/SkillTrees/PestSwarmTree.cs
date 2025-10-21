using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.SkillPassives;
using PathOfTerraria.Content.SkillPassives.SwarmPassives;
using PathOfTerraria.Content.Skills.Summon;
using PathOfTerraria.Content.SkillSpecials.PestSwarmSpecials;

namespace PathOfTerraria.Content.SkillTrees;

internal class PestSwarmTree : SkillTree
{
	public override Type ParentSkill => typeof(Swarm);

	public override void Populate()
	{
		var anchor = new Anchor(this) { Level = 1 };
		var swarm = new AntlionSwarm(this) { TreePos = new Vector2(200, 0) };
		var locusts = new LocustBrood(this) { TreePos = new Vector2(0, -200) };
		var glacier = new GlacialAntlions(this) { TreePos = new Vector2(0, 200) };
		AddNodes(anchor, swarm, locusts, glacier);

		var brood = new BiggerBrood(this) { TreePos = new Vector2(0, -300), MaxLevel = 2 };
		var gest = new Gestation(this) { TreePos = new Vector2(100, -200) };
		var egg = new Eggsplosion(this) { TreePos = new Vector2(-100, -200) };
		AddNodes(locusts, brood, gest, egg);
		AddNodes(egg, new ShockingEmergence(this) { TreePos = new Vector2(-100, -100) });
		AddNodes(gest, new QuickerHatching(this) { TreePos = new Vector2(200, -200), MaxLevel = 2 }, new CarnivorousLarvae(this) { TreePos = new Vector2(100, -300) });

		var vola = new VolatileInsects(this) { TreePos = new Vector2(300, 0) };
		AddNodes(swarm, vola, new ViciousBites(this) { TreePos = new Vector2(200, -100) }, new CarapaceCracker(this) { TreePos = new Vector2(200, 100) });

		var inf = new InfectedDetonation(this) { TreePos = new Vector2(400, 0) };
		var over = new OverheatingBugs(this) { TreePos = new Vector2(300, 100) };
		AddNodes(vola, inf, over);
		AddNodes(inf, new HeartierExplosions(this) { TreePos = new Vector2(400, 100), MaxLevel = 2 }, new CombustableGuts(this) { TreePos = new Vector2(400, -100) });
		AddNodes(over, new SuperheatedBugs(this) { TreePos = new Vector2(300, 200) } );

		var cold = new ColdBlooded(this) { TreePos = new Vector2(100, 200) };
		var frost = new FrostbiteMandibles(this) { TreePos = new Vector2(0, 300) };
		var shatter = new ShatteringCarapace(this) { TreePos = new Vector2(-100, 200) };
		AddNodes(glacier, cold, frost, shatter);
		AddNodes(cold, new ThermalConversion(this) { TreePos = new Vector2(100, 100), MaxLevel = 2 });
		AddNodes(frost, new AggressiveChill(this) { TreePos = new Vector2(100, 300), MaxLevel = 2 }, new IceVenom(this) { TreePos = new Vector2(0, 400) });
	}
}