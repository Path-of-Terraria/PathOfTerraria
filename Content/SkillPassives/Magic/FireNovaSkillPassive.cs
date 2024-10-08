using PathOfTerraria.Common.Mechanics;

namespace PathOfTerraria.Content.SkillPassives.Magic;

public sealed class FireNovaSkillPassive : SkillPassive
{
	public override int ReferenceId => 1;
	public override int MaxLevel => 1;
	public override Vector2 TreePos => new(100, 0);
	
	public FireNovaSkillPassive(Skill skill) : base(skill)
	{
		Connections = [new SkillPassiveAnchor(skill)];
	}

	public override void LevelTo(byte level)
	{
		Level = level;
	}
}