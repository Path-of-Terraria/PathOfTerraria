using PathOfTerraria.Common.Mechanics;
using PathOfTerraria.Common.Systems.Skills;

namespace PathOfTerraria.Content.SkillPassives.FireballPassives;

internal class Rime(SkillTree tree) : SkillPassive(tree)
{
	internal const float SizeBoost = 0.33f;

	public override object[] TooltipArguments => [$"{SizeBoost * 100:#0}"];
}
