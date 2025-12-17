using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.FireballPassives;

internal class EverburningPyre(SkillTree tree) : SkillPassive(tree)
{
	public const float DurationModifier = 0.2f;

	public override object[] TooltipArguments => [$"{DurationModifier * 100:#0}"];
}
