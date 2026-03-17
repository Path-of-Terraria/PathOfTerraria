using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.FireballPassives;

internal class Pyroclasm(SkillTree tree) : SkillPassive(tree)
{
	internal const float DamageBoost = 0.25f;

	public override object[] TooltipArguments => [$"{DamageBoost * 100:#0}"];
}
