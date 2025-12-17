using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.FireballPassives;

internal class Pyremaniac(SkillTree tree) : SkillPassive(tree)
{
	public const float DoTBuff = 15f;

	public override object[] TooltipArguments => [$"{(DoTBuff / 30f) * 100:#0}"];
}
