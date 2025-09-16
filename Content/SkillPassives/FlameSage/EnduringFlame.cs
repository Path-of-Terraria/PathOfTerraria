using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillPassives.FlameSage;

internal class EnduringFlame(SkillTree tree) : SkillPassive(tree)
{
	public const float DamageBonus = 0.02f;
	public override string DisplayTooltip => string.Format(base.DisplayTooltip, MathUtils.Percent(DamageBonus));
}