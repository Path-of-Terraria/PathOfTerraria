using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Content.Skills.Magic;

namespace PathOfTerraria.Content.SkillPassives.Magic;

internal class NovaBlocker : SkillPassiveBlocker
{
	public override bool BlockAllocation(SkillPassive passive)
	{
		return passive.Skill is Nova nova && Nova.GetNovaType(nova) != Nova.NovaType.Normal;
	}
}
