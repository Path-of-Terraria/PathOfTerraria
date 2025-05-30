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
		var novaFire = new FireNova(this);
		var novaIce = new IceNova(this);
		var novaLightning = new LightningNova(this);
		var efficiency = new Efficiency(this);
		var thunderClaps = new ThunderClaps(this);
		var concurrentBlasts = new ConcurrentBlasts(this);
		var volatileNova = new VolatileNova(this);
		var igniteChance = new IgniteChance(this);
		var shockChance = new ShockChance(this);

		Nodes.Add(new Vector2(0, 0), anchor);
		Nodes.Add(new Vector2(-200, 0), novaFire);
		Nodes.Add(new Vector2(0, 100), novaIce);
		Nodes.Add(new Vector2(200, 0), novaLightning);

		Nodes.Add(new Vector2(50, -50), efficiency);
		Nodes.Add(new Vector2(200, -80), thunderClaps);
		Nodes.Add(new Vector2(80, 150), concurrentBlasts);
		Nodes.Add(new Vector2(80, 100), volatileNova);
		Nodes.Add(new Vector2(-200, 80), igniteChance);
		Nodes.Add(new Vector2(250, 80), shockChance);

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