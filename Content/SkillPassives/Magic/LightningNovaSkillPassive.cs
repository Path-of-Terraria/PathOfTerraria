using PathOfTerraria.Common.Mechanics;

namespace PathOfTerraria.Content.SkillPassives.Magic;

public sealed class LightningNovaSkillPassive : SkillPassive
{
	public override int ReferenceId => 3;
	public override int MaxLevel => 1;
	public override Vector2 TreePos => new(0, -100);
	
	public LightningNovaSkillPassive(Skill skill) : base(skill)
	{
		Connections = [new SkillPassiveAnchor(skill)];
	}
	
	public override void LevelTo(byte level)
	{
		Level = level;
	}
}