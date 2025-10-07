using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillPassives.FlameSage;

internal class ImprovedWarding : SkillPassive
{
	public const float ResistanceIncrease = 0.05f;
	public override object[] TooltipArguments => [MathUtils.Percent(ResistanceIncrease)];

	public ImprovedWarding(SkillTree tree) : base(tree)
	{
		MaxLevel = 2;
	}
}