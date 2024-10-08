using System.Collections.Generic;
using PathOfTerraria.Common.Data.Models;
using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Content.Skills.Magic;

namespace PathOfTerraria.Content.SkillPassives.Magic;

public sealed class IceNovaSkillPassive : SkillPassive
{
	public override int ReferenceId => 2;
	public override int MaxLevel => 1;
	public override Vector2 TreePos => new(0, 100);
	
	public IceNovaSkillPassive(Skill skill) : base(skill)
	{
		Connections = [new SkillPassiveAnchor(skill)];
	}
	
	public override void LevelTo(byte level)
	{
		Level = level;
	}
}