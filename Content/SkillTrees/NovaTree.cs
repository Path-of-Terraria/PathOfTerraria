using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.SkillPassives;
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

		Nodes.Add(anchor);
		Nodes.Add(novaFire);
		Nodes.Add(novaIce);
		Nodes.Add(novaLightning);

		Nodes.Add(efficiency);
		Nodes.Add(thunderClaps);
		Nodes.Add(concurrentBlasts);
		Nodes.Add(volatileNova);
		Nodes.Add(igniteChance);
		Nodes.Add(shockChance);

		Edges.Add(new(novaFire, anchor));
		Edges.Add(new(novaIce, anchor));
		Edges.Add(new(novaLightning, anchor));
		Edges.Add(new(efficiency, anchor));
		Edges.Add(new(thunderClaps, novaLightning));
		Edges.Add(new(igniteChance, novaFire));
		Edges.Add(new(shockChance, novaLightning));
		Edges.Add(new(volatileNova, novaIce));
		Edges.Add(new(volatileNova, novaLightning));
		Edges.Add(new(concurrentBlasts, volatileNova));
	}
}