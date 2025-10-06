using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;
using PathOfTerraria.Common.Utilities;

namespace PathOfTerraria.Content.SkillPassives.FlameSage;

internal class Weakening : SkillPassive
{
	public const float ResistanceDecrease = 0.01f;
	public override object[] TooltipArguments => [MathUtils.Percent(ResistanceDecrease)];

	public Weakening(SkillTree tree) : base(tree)
	{
		MaxLevel = 2;
	}
}