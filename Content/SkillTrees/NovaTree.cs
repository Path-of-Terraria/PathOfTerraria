using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.SkillPassives;
using PathOfTerraria.Content.SkillPassives.Generic;
using PathOfTerraria.Content.SkillPassives.NovaTree;
using PathOfTerraria.Content.Skills.Magic;
using PathOfTerraria.Content.SkillSpecials;

namespace PathOfTerraria.Content.SkillTrees;

internal class NovaTree : SkillTree
{
	public override Type ParentSkill => typeof(Nova);

	public override void Populate() //Could be moved to a .json based system at some point, like player passives.
	{
		var anchor = new Anchor(this) { Level = 1 };
		var novaFire = new FireNova(this) { TreePos = new Vector2(-200, 0) };
		var novaIce = new IceNova(this) { TreePos = new Vector2(0, 100) };
		var novaLightning = new LightningNova(this) { TreePos = new Vector2(200, 0) };
		var efficiency = new Efficiency(this) { TreePos = new Vector2(50, -50) };
		var thunderClaps = new ThunderClaps(this) { TreePos = new Vector2(200, -80) };
		var concurrentBlasts = new ConcurrentBlasts(this) { TreePos = new Vector2(80, 150) };
		var volatileNova = new VolatileNova(this) { TreePos = new Vector2(80, 100) };
		var igniteChance = new IgniteChance(this) { TreePos = new Vector2(-200, 80) };
		var shockChance = new ShockChance(this) { TreePos = new Vector2(250, 80) };
		var flashFire = new FlashFire(this) { TreePos = new Vector2(-200, -100) };
		var combustive = new Combustive(this) { TreePos = new Vector2(-300, -100) };
		var scorching = new ScorchingTouch(this) { TreePos = new Vector2(-340, 100) };

		AddNodes(anchor, novaFire, novaIce, novaLightning, efficiency);
		AddNodes(novaLightning, thunderClaps);
		AddNodes(novaFire, igniteChance, flashFire, combustive, scorching);
		AddNodes(novaLightning, shockChance);
		AddNodes(novaIce, volatileNova);
		AddNodes(volatileNova, novaLightning);
		AddNodes(concurrentBlasts, volatileNova);
	}
}