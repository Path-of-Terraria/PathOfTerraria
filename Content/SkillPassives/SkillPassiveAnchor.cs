using PathOfTerraria.Common.Mechanics;

namespace PathOfTerraria.Content.SkillPassives;

public class SkillPassiveAnchor : SkillPassive
{
	public override int ReferenceId => 0;
	public override int MaxLevel => 0;
	public override string Name => "Anchor";
	public override Vector2 TreePos => new(0, 0);

	public SkillPassiveAnchor(Skill skill) : base(skill)
	{
		Level = 1;
	}

	public override void LevelTo(byte level) { }
}