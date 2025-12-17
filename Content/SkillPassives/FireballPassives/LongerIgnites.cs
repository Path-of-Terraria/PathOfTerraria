using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.FireballPassives;

internal class LongerIgnites(SkillTree tree) : SkillPassive(tree)
{
	public const float DamageBoost = 0.5f;

	public override object[] TooltipArguments => [$"{DamageBoost * 100:#0}"];
}
