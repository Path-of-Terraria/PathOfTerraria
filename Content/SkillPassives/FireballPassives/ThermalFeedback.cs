using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.FireballPassives;

internal class ThermalFeedback(SkillTree tree) : SkillPassive(tree)
{
	public const int ManaHealed = 5;

	public override object[] TooltipArguments => [ManaHealed];
}
