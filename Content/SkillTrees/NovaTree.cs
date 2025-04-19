using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.SkillPassives;
using PathOfTerraria.Content.Skills.Magic;
using PathOfTerraria.Content.SkillSpecials;

namespace PathOfTerraria.Content.SkillTrees;

internal class NovaTree : SkillTree
{
	public override Type ParentSkill => typeof(Nova);

	public override void Populate()
	{
		var passive = new SkillPassiveAnchor();
		var novaFire = new FireNova();
		var novaIce = new IceNova();
		var novaLightning = new LightningNova();
		var strength = new Strength();

		Allocatables.Add(new Vector2(0, 0), passive);
		Allocatables.Add(new Vector2(100, 0), novaFire);
		Allocatables.Add(new Vector2(200, 0), novaIce);
		Allocatables.Add(new Vector2(300, 0), novaLightning);
		Allocatables.Add(new Vector2(100), strength);

		Edges.Add(new(passive, strength));
		Edges.Add(new(strength, novaLightning));
	}
}