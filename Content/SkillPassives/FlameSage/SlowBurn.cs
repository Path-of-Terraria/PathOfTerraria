using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.FlameSage;

internal class SlowBurn(SkillTree tree) : SkillPassive(tree)
{
	public const float Seconds = 4;
	public override string DisplayTooltip => string.Format(base.DisplayTooltip, Seconds);
}