using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillPassives.FlameSage;

internal class ImprovedBurning : SkillPassive
{
	public const float DamageIncrease = 0.05f;
	public override object[] TooltipArguments => [MathUtils.Percent(DamageIncrease)];

	public ImprovedBurning(SkillTree tree) : base(tree)
	{
		MaxLevel = 3;
	}
}