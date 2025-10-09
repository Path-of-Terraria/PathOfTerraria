using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillPassives.FlameSage;

internal class HeatCompression(SkillTree tree) : SkillPassive(tree)
{
	public float RadiusReduction = 0.5f;
	public const float DamageBonus = 0.25f;

	public override object[] TooltipArguments => [MathUtils.Percent(RadiusReduction), MathUtils.Percent(DamageBonus)];
}