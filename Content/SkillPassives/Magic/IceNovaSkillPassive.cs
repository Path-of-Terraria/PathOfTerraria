using PathOfTerraria.Common.Mechanics;

namespace PathOfTerraria.Content.SkillPassives.Magic;

public sealed class IceNovaSkillPassive(Skill skill) : SkillPassive(skill)
{
	public override int ReferenceId => 2;
	public override int MaxLevel => 1;
	public override Vector2 TreePos => new(0, 100);
	
	public override void LevelTo(byte level)
	{
		Level = level;
	}
}