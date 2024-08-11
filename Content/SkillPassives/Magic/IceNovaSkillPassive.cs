using System.Collections.Generic;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Content.Skills.Magic;

namespace PathOfTerraria.Content.SkillPassives.Magic;

public class IceNovaSkillPassive : SkillPassive
{
	public override Skill Skill => new Nova();
	public override int ReferenceId => 2;
	public override int MaxLevel => 1;
	public override string Name => "Ice Nova";
	public override Vector2 TreePos => new(0, 100);
	
	public override List<SkillPassive> Connections =>
	[
		new SkillPassiveAnchor()
	];
	
	public override void LevelTo(byte level)
	{
		Level = level;
	}
}