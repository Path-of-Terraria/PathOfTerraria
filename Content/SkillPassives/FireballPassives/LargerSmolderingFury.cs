using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.FireballPassives;

internal class LargerSmolderingFury(SkillTree tree) : SkillPassive(tree)
{
	public const float SizeBoost = 0.5f;

	public override object[] TooltipArguments => [$"{SizeBoost * 100:#0}"];
}
