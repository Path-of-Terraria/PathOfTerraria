using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.FireballPassives;

internal class ShadowflameEruption(SkillTree tree) : SkillPassive(tree)
{
	public const float DamageBuff = 0.5f;

	public override object[] TooltipArguments => [$"{DamageBuff * 100:#0}"];
}
