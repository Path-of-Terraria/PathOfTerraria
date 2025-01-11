using PathOfTerraria.Common.Mechanics;

namespace PathOfTerraria.Content.SkillPassives.Magic;

public sealed class LightningNovaSkillPassive(Skill skill) : SkillPassive(skill)
{
	public override int ReferenceId => 3;
	public override int MaxLevel => 1;
	public override Vector2 TreePos => new(0, -100);
	
	public override void LevelTo(byte level)
	{
		Level = level;
	}
}