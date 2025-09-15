using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillPassives.FlameSage;

internal class MeltingPoint(SkillTree tree) : SkillPassive(tree)
{
	public const float DamageBonus = 0.2f;
	public const float Seconds = 3f;

	public override string DisplayTooltip => string.Format(base.DisplayTooltip, MathUtils.Percent(DamageBonus), Seconds);
}