using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.SkillPassives;
using PathOfTerraria.Content.SkillPassives.FireballPassives;
using PathOfTerraria.Content.Skills.Magic;
using PathOfTerraria.Content.SkillSpecials.FireballSpecials;

namespace PathOfTerraria.Content.SkillTrees;

internal class FireballTree : SkillTree
{
	public override Type ParentSkill => typeof(Fireball);

	public override void Populate()
	{
		var anchor = new Anchor(this) { Level = 1 };
		
		// ─────────────────────────────────────────────
		// INFERNO SPEC
		// ─────────────────────────────────────────────
		var inferno = new Inferno(this) { TreePos = new Vector2(-200, 0) };

		var fireNova = new FireNova(this) { TreePos = new Vector2(-300, 0) };
		var ignitesLastLonger = new LongerIgnites(this) { TreePos = new Vector2(-400, 0) };
		var pyroclasm = new Pyroclasm(this) { TreePos = new Vector2(-300, -100) };

		var smolderingFury = new SmolderingFury(this) { TreePos = new Vector2(-200, 100) };
		var strongerSmoldering = new StrongerSmolderingFury(this) { TreePos = new Vector2(-100, 100) };
		var igniteSlow = new SlowingSmolderingFury(this) { TreePos = new Vector2(-200, 200) };
		var igniteProlifRange = new LargerSmolderingFury(this) { TreePos = new Vector2(-300, 100) };

		var scorchedEarth = new ScorchedEarth(this) { TreePos = new Vector2(-200, -100) };
		var longerScorched = new LongerScorchedEarth(this) { TreePos = new Vector2(-100, -100) };
		var strongerScorched = new StrongerScorchedEarth(this) { TreePos = new Vector2(-200, -200) };
		
		// ─────────────────────────────────────────────
		// FROSTFIRE METEOR SPEC
		// ─────────────────────────────────────────────
		var frostfireMeteors = new FrostfireMeteor(this) { TreePos = new Vector2(0, 100) };

		var crystallineImpact = new CrystallineImpact(this) { TreePos = new Vector2(0, 200) };

		var rime = new Rime(this) { TreePos = new Vector2(200, 200) };

		var everburningFrost = new EverburningFrost(this) { TreePos = new Vector2(0, 300) };
		var frostfireBlast = new FrostfireBlast(this) { TreePos = new Vector2(0, 400)  };
		var frozenGround = new FrozenGround(this) { TreePos = new Vector2(-100, 300) };

		var coldFocus = new ColdFocus(this) { TreePos = new Vector2(100, 300) };
		var absoluteZero = new AbsoluteZero(this) { TreePos = new Vector2(200, 300) };
		var thermalFeedback = new ThermalFeedback(this) { TreePos = new Vector2(200, 400) };
	
		// ─────────────────────────────────────────────
		// SHADOWFLAME PYRE SPEC
		// ─────────────────────────────────────────────
		var shadowflamePyre = new ShadowflamePyre(this) { TreePos = new Vector2(200, 0) };

		var painIsPleasure = new PainIsPleasure(this) { TreePos = new Vector2(200, 100) };
		var everburningPyre = new EverburningPyre(this) { TreePos = new Vector2(200, -100) };

		var crawlingFlame = new CrawlingFlame(this) { TreePos = new Vector2(300, 0) };
		var abyssalHunger = new Abyssalhunger(this) { TreePos = new Vector2(300, -100) };
		
		var pyremaniac = new Pyremaniac(this) { TreePos = new Vector2(400, 0) };
		var additionalPyres = new AdditionalPyres(this) { TreePos = new Vector2(400, -100) };
		var conjoinedFlames = new ConjoinedFlames(this) { TreePos = new Vector2(400, 100) };
		var shadowflameEruption = new ShadowflameEruption(this) { TreePos = new Vector2(400, 200) };
		
		AddNodes(anchor, inferno, frostfireMeteors, shadowflamePyre);;

		AddNodes(inferno, fireNova, smolderingFury, scorchedEarth);
		AddNodes(fireNova, ignitesLastLonger);
		AddNodes(smolderingFury, strongerSmoldering, igniteProlifRange, igniteSlow);
		AddNodes(scorchedEarth, longerScorched, strongerScorched, pyroclasm);
		
		AddNodes(frostfireMeteors, crystallineImpact);
		AddNodes(crystallineImpact, rime, everburningFrost);
		AddNodes(everburningFrost, frostfireBlast, coldFocus, frozenGround);
		AddNodes(coldFocus, absoluteZero);
		AddNodes(absoluteZero, rime, thermalFeedback);

		AddNodes(shadowflamePyre, everburningPyre, painIsPleasure, crawlingFlame);
		AddNodes(crawlingFlame, pyremaniac, abyssalHunger);
		AddNodes(conjoinedFlames, shadowflameEruption, pyremaniac);
		AddNodes(pyremaniac, additionalPyres);
	}
}