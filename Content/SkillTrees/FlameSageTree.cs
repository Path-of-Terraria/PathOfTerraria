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
		var heatCompression = new HeatCompression(this) { TreePos = new Vector2(-200, -100) };
		var combustiveAura = new CombustiveAura(this) { TreePos = new Vector2(-300, 0) };
		var meltingPoint = new MeltingPoint(this) { TreePos = new Vector2(-200, 100) };

		var moltenSentinel = new MoltenSentinel(this) { TreePos = new Vector2(0, 200) };
		var intenseHeat = new IntenseHeat(this) { TreePos = new Vector2(-100, 200) };
		var flameWard = new FlameWard(this) { TreePos = new Vector2(100, 200) };
		var livingFurnace = new LivingFurnace(this) { TreePos = new Vector2(0, 300) };

		var volatileConstruct = new VolatileConstruct(this) { TreePos = new Vector2(200, 0) };
		var slowBurn = new SlowBurn(this) { TreePos = new Vector2(200, -100) };

		AddNodes(anchor, flamethrower, moltenSentinel, volatileConstruct);
		AddNodes(moltenSentinel, intenseHeat, flameWard, livingFurnace);
		AddNodes(flamethrower, heatCompression, combustiveAura, meltingPoint);
		AddNodes(volatileConstruct, slowBurn);
	}
}