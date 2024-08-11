using PathOfTerraria.Common.Mechanics;

namespace PathOfTerraria.Content.SkillPassives.Magic;

public class LightningNovaSkillPassive : SkillPassive
{
	public override int MaxLevel => 1;
	
	public override void LevelTo(byte level)
	{
		Level = level;
	}
}