using System.Collections.Generic;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Content.Skills.Magic;

namespace PathOfTerraria.Content.SkillPassives.Magic;

public class FireNovaSkillPassive : SkillPassive
{
	public override Skill Skill => new Nova();
	public override int ReferenceId => 1;
	public override int MaxLevel => 1;
	public override Vector2 TreePos => new(100, 0);

	public override List<SkillPassive> Connections =>
	[
		new SkillPassiveAnchor()
	];

	public override void LevelTo(byte level)
	{
		Level = level;
	}
}