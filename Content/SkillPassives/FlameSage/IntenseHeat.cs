using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillPassives.FlameSage;

internal class IntenseHeat(SkillTree tree) : SkillPassive(tree)
{
	public const float DamageIncrease = 0.2f;
	public override string DisplayTooltip => string.Format(base.DisplayTooltip, MathUtils.Percent(DamageIncrease));
}