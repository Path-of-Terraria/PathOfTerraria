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
		var passive = new Anchor(this);
		var novaFire = new FireNova(this);
		var novaIce = new IceNova(this);
		var novaLightning = new LightningNova(this);
		var strength = new Strength(this);

		Nodes.Add(new Vector2(0, 0), passive);
		Nodes.Add(new Vector2(100, 0), novaFire);
		Nodes.Add(new Vector2(200, 0), novaIce);
		Nodes.Add(new Vector2(300, 0), novaLightning);
		Nodes.Add(new Vector2(100), strength);

		Edges.Add(new(passive, strength));
		Edges.Add(new(strength, novaLightning));
	}
}