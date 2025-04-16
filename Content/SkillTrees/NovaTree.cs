using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Content.SkillPassives;
using PathOfTerraria.Content.SkillSpecials;

namespace PathOfTerraria.Content.SkillTrees;

internal class NovaTree : SkillTree
{
	public override void Populate()
	{
		var passive = new SkillPassiveAnchor(this);
		var novaFire = new FireNova(this);
		var novaIce = new IceNova(this);
		var novaLightning = new LightningNova(this);
		var strength = new Strength(this);

		Allocatables.Add(new Vector2(0, 0), passive);
		Allocatables.Add(new Vector2(100, 0), novaFire);
		Allocatables.Add(new Vector2(200, 0), novaIce);
		Allocatables.Add(new Vector2(300, 0), novaLightning);
		Allocatables.Add(new Vector2(100), strength);

		Edges.Add(new(passive, strength));
	}
}