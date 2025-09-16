using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillPassives.FlameSage;

internal class Accelerant(SkillTree tree) : SkillPassive(tree)
{
	public const float DamageIncrease = 0.15f;
	public const float LifeLoss = 0.05f;

	public override string DisplayTooltip => string.Format(base.DisplayTooltip, MathUtils.Percent(LifeLoss), MathUtils.Percent(DamageIncrease));
}