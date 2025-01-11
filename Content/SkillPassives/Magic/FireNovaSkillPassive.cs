using PathOfTerraria.Common.Mechanics;

namespace PathOfTerraria.Content.SkillPassives.Magic;

public sealed class FireNovaSkillPassive(Skill skill) : SkillPassive(skill)
{
	public override int ReferenceId => 1;
	public override int MaxLevel => 1;
	public override Vector2 TreePos => new(100, 0);
	
	public override void LevelTo(byte level)
	{
		Level = level;
	}
}